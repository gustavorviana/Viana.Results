using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Viana.Results.OpenApi.Schemas;

namespace Viana.Results.OpenApi.Processing;

/// <summary>
/// Shared per-operation mutations applied to a <see cref="OpenApiOperation"/>.
/// Used by both the Swashbuckle <c>IOperationFilter</c> implementations and the
/// <c>Microsoft.AspNetCore.OpenApi</c> <c>IOpenApiOperationTransformer</c>
/// implementations to keep the logic in one place.
/// </summary>
internal static class OperationProcessor
{
    /// <summary>
    /// When the action returns the non-generic <see cref="Result"/>, clears all response
    /// content (Result has no body — only the HTTP status code).
    /// </summary>
    internal static void ClearBodyForBareResult(OpenApiOperation operation, MethodInfo method)
    {
        if (operation?.Responses is null || operation.Responses.Count == 0)
            return;

        var returnType = UnwrapTaskType(method.ReturnType);

        if (returnType != typeof(Result))
            return;

        foreach (var response in operation.Responses.Values)
        {
            if (response.Content is null || response.Content.Count == 0)
                continue;

            response.Content.Clear();
            response.Description ??= "OK";
        }
    }

    /// <summary>
    /// Adds RFC 9457 problem responses for every <see cref="ProblemResultAttribute"/> declared
    /// on the action or its declaring type.
    /// </summary>
    internal static void ApplyProblemResultAttributes(OpenApiOperation operation, MethodInfo method, JsonSerializerOptions json)
    {
        var returnType = UnwrapTaskType(method.ReturnType);

        if (!typeof(IResult).IsAssignableFrom(returnType))
            return;

        foreach (var problem in GetProblemAttributes(method))
            ProblemResultSchema.FromAttribute(json, problem).ApplyToCore(operation.Responses);
    }


    // Same rule as ASP.NET Core: authorized when IAuthorizeData is present and IAllowAnonymous is not.
    // Endpoint metadata covers controllers ([Authorize]/[AllowAnonymous]) and minimal APIs
    // (RequireAuthorization()/AllowAnonymous()); without metadata, the method and its class are inspected.
    internal static void ApplyAuthorizeResponses(OpenApiOperation operation, IEnumerable<object>? endpointMetadata, MethodInfo? method, JsonSerializerOptions json)
    {
        var metadata = endpointMetadata?.ToList();
        if (metadata == null || metadata.Count == 0)
        {
            metadata = new List<object>();
            if (method != null)
            {
                if (method.DeclaringType != null)
                    metadata.AddRange(method.DeclaringType.GetCustomAttributes(true));
                metadata.AddRange(method.GetCustomAttributes(true));
            }
        }

        var requiresAuthorization = metadata.OfType<Microsoft.AspNetCore.Authorization.IAuthorizeData>().Any()
            && !metadata.OfType<Microsoft.AspNetCore.Authorization.IAllowAnonymous>().Any();

        if (!requiresAuthorization)
            return;

        new ProblemResultSchema(json, 401).ApplyToCore(operation.Responses);
        new ProblemResultSchema(json, 403).ApplyToCore(operation.Responses);
    }

    // Task<T> and ValueTask<T> are unwrapped by generic type definition (not by name).
    internal static Type UnwrapTaskType(Type type)
    {
        if (type.IsGenericType)
        {
            var definition = type.GetGenericTypeDefinition();
            if (definition == typeof(System.Threading.Tasks.Task<>) || definition == typeof(System.Threading.Tasks.ValueTask<>))
                return type.GetGenericArguments()[0];
        }

        return type;
    }

    /// <summary>
    /// Materializes <see cref="ResponseExampleAttribute"/> declarations (and globals from
    /// <see cref="VianaResultOptions"/>) into <c>example</c>/<c>examples</c> on each JSON
    /// response media type.
    /// </summary>
    internal static void ApplyResponseExamples(
        OpenApiOperation operation,
        MethodInfo method,
        JsonSerializerOptions json,
        IServiceProvider? services,
        VianaResultOptions? options)
    {
        if (operation.Responses == null || operation.Responses.Count == 0)
            return;

        var globals = options?.GlobalExamples ?? Array.Empty<GlobalExampleRegistration>();
        var byStatus = ExampleResolver.Collect(method, globals);
        if (byStatus.Count == 0)
            return;

        var declared = new HashSet<int>(GetProblemAttributes(method).Select(a => a.Status));

        foreach (var pair in byStatus)
        {
            var statusKey = pair.Key.ToString();
            var built = BuildExamples(pair.Value, json, services);
            if (built.Count == 0)
                continue;

            // FR-014: a problem example defines schema and example of its status. Endpoint examples win over
            // attributes; a global example is only a fallback when the endpoint declares nothing for the status.
            var problems = built.Where(b => b.Problem != null).ToList();
            if (problems.Count > 0 && (!problems[0].Meta.IsGlobal || !declared.Contains(pair.Key)))
            {
                var schema = new ProblemResultSchema(json, problems[0].Problem!);
                foreach (var other in problems.Skip(1))
                {
                    foreach (var member in other.Problem!.Extensions)
                        schema.AddFieldCore(member.Value?.GetType(), member.Key, "", member.Value);
                }
                operation.Responses[statusKey] = schema.BuildCore([ProblemResultSchema.ProblemContentType]);
                ApplyExamplesToContent(operation.Responses[statusKey].Content!, problems);
                continue;
            }

            if (problems.Count > 0)
                continue; // global problem example, but the endpoint declared this status

            if (!operation.Responses.TryGetValue(statusKey, out var response) || response.Content is not { } content)
                continue;

            ApplyExamplesToContent(content, built);
        }
    }

    private sealed record BuiltExample(string Key, ExampleRegistration Meta, System.Text.Json.Nodes.JsonNode? Node, ProblemResult? Problem);

    private static List<BuiltExample> BuildExamples(List<ExampleRegistration> examples, JsonSerializerOptions json, IServiceProvider? services)
    {
        var built = new List<BuiltExample>();
        foreach (var ex in examples)
        {
            var provider = ExampleResolver.CreateProvider(ex.ProviderType, services);
            if (provider == null)
                continue;

            var value = ExampleResolver.InvokeGetExample(provider);
            var problem = value as ProblemResult ?? (value as IResult)?.Problem;
            var node = ExampleResolver.Serialize((object?)problem ?? value, json);
            built.Add(new BuiltExample(ex.Name ?? ex.ProviderType.Name, ex, node, problem));
        }
        return built;
    }

    private static void ApplyExamplesToContent(IDictionary<string, OpenApiMediaType> content, List<BuiltExample> built)
    {
        if (built.Count == 0)
            return;

        var useExamplesMap = built.Count > 1 || built.Any(b => b.Meta.Name != null);

        foreach (var media in content.Values)
        {
            if (useExamplesMap)
            {
                media.Example = null;
                media.Examples ??= new Dictionary<string, IOpenApiExample>();
                foreach (var (key, meta, node, _) in built)
                {
                    media.Examples[key] = new OpenApiExample
                    {
                        Value = node,
                        Summary = meta.Summary,
                        Description = meta.Description
                    };
                }
            }
            else
            {
                media.Example = built[0].Node;
            }
        }
    }

    // FR-015: one attribute per status, the action's wins over the controller's (like native ApiExplorer).
    private static List<ProblemResultAttribute> GetProblemAttributes(MethodInfo method)
    {
        var attributes = new List<ProblemResultAttribute>(method.GetCustomAttributes<ProblemResultAttribute>(true));
        var statuses = new HashSet<int>(attributes.Select(a => a.Status));
        if (method.DeclaringType != null)
        {
            foreach (var attribute in method.DeclaringType.GetCustomAttributes<ProblemResultAttribute>(true))
            {
                if (statuses.Add(attribute.Status))
                    attributes.Add(attribute);
            }
        }
        return attributes;
    }
}
