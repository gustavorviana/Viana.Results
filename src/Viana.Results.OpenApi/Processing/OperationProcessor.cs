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

        foreach (var pair in byStatus)
        {
            var statusKey = pair.Key.ToString();
            if (!operation.Responses.TryGetValue(statusKey, out var response) || response.Content is not { } content)
                continue;

            ApplyExamplesToContent(content, pair.Value, json, services);
        }
    }

    private static void ApplyExamplesToContent(IDictionary<string, OpenApiMediaType> content, List<ExampleRegistration> examples, JsonSerializerOptions json, IServiceProvider? services)
    {
        var built = new List<(string Key, ExampleRegistration Meta, System.Text.Json.Nodes.JsonNode? Node)>();

        foreach (var ex in examples)
        {
            var provider = ExampleResolver.CreateProvider(ex.ProviderType, services);
            if (provider == null)
                continue;

            var value = ExampleResolver.InvokeGetExample(provider);
            var node = ExampleResolver.Serialize(value, json);
            var key = ex.Name ?? ex.ProviderType.Name;
            built.Add((key, ex, node));
        }

        if (built.Count == 0)
            return;

        var useExamplesMap = built.Count > 1 || built.Any(b => b.Meta.Name != null);

        foreach (var media in content.Values)
        {
            if (useExamplesMap)
            {
                media.Example = null;
                media.Examples ??= new Dictionary<string, IOpenApiExample>();
                foreach (var (key, meta, node) in built)
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

    private static List<ProblemResultAttribute> GetProblemAttributes(MethodInfo method)
    {
        var attributes = new List<ProblemResultAttribute>();
        attributes.AddRange(method.GetCustomAttributes<ProblemResultAttribute>(true));
        if (method.DeclaringType != null)
            attributes.AddRange(method.DeclaringType.GetCustomAttributes<ProblemResultAttribute>(true));
        return attributes;
    }
}
