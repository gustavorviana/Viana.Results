using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
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

        // Feature 008: providers may return real Viana results; their status (and body) comes from the result,
        // so examples are regrouped by the resolved status (attributes without status use 0 until resolved).
        var regrouped = byStatus.Values
            .SelectMany(list => BuildExamples(list, json, services, method))
            .GroupBy(b => b.Status);

        foreach (var pair in regrouped)
        {
            var statusKey = pair.Key.ToString();
            var built = pair.ToList();
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

    private sealed record BuiltExample(string Key, ExampleRegistration Meta, System.Text.Json.Nodes.JsonNode? Node, ProblemResult? Problem, int Status = 0);

    private static List<BuiltExample> BuildExamples(List<ExampleRegistration> examples, JsonSerializerOptions json, IServiceProvider? services, MethodInfo method)
    {
        var built = new List<BuiltExample>();
        foreach (var ex in examples)
        {
            var provider = ExampleResolver.CreateProvider(ex.ProviderType, services);
            if (provider == null)
                continue;

            var value = ExampleResolver.InvokeGetExample(provider);
            var key = ex.Name ?? ex.ProviderType.Name;
            if (value is IResult result)
            {
                if (ex.StatusCode != 0 && ex.StatusCode != result.Status)
                    throw new InvalidOperationException(
                        $"Endpoint '{method.DeclaringType?.Name}.{method.Name}': the declared example status {ex.StatusCode} differs from the example result status {result.Status} ({ex.ProviderType.Name}).");

                var body = result.Problem ?? ResultResponseBody.GetBody(result);
                built.Add(new BuiltExample(key, ex, ExampleResolver.Serialize(body, json), result.Problem, result.Status));
                continue;
            }

            var problem = value as ProblemResult;
            built.Add(new BuiltExample(key, ex, ExampleResolver.Serialize(value, json), problem, ex.StatusCode != 0 ? ex.StatusCode : problem?.Status ?? 200));
        }
        return built;
    }

    private static void ApplyExamplesToContent(IDictionary<string, OpenApiMediaType> content, List<BuiltExample> built)
    {
        if (built.Count == 0)
            return;

        // FR-011: a problem example without its own type documents the native link the runtime sends.
        foreach (var b in built.Where(b => b.Problem != null))
            Schemas.NativeProblemTypes.Apply(b.Node, b.Status != 0 ? b.Status : b.Problem!.Status);

        var useExamplesMap = built.Count > 1 || built.Any(b => b.Meta.Name != null);

        foreach (var media in content.Values)
        {
            if (useExamplesMap)
            {
                media.Example = null;
                media.Examples ??= new Dictionary<string, IOpenApiExample>();
                foreach (var (key, meta, node, _, _) in built)
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

    private sealed record ResolvedDeclaration(VianaResponseMetadata Declaration, int Status, bool HasExample, ProblemResult? Problem, object? Body, bool WritesBody);

    /// <summary>
    /// Feature 008: documents the responses declared on a minimal API endpoint through
    /// <see cref="VianaResponseMetadata"/> (ProducesResult / ProducesProblemResult). Runs after the
    /// attribute/global processing so endpoint declarations win; within the endpoint, examples win over
    /// metadata-only declarations for the same status. Async end to end: <paramref name="schemaFor"/> is the
    /// pipeline's schema generator (native: awaited; Swashbuckle: already-completed ValueTask).
    /// </summary>
    internal static async ValueTask ApplyEndpointDeclarationsAsync(
        OpenApiOperation operation,
        IEnumerable<object>? metadata,
        JsonSerializerOptions json,
        IServiceProvider? services,
        Func<Type, ValueTask<IOpenApiSchema>> schemaFor,
        string endpointName)
    {
        var resolved = ResolveDeclarations(metadata, services, endpointName);
        if (resolved.Count == 0)
            return;

        // Native pipeline: generate the needed schemas asynchronously, then build synchronously.
        var schemas = new Dictionary<Type, IOpenApiSchema>();
        foreach (var type in RequiredSchemaTypes(resolved))
            schemas[type] = await schemaFor(type).ConfigureAwait(false);

        BuildDeclarations(operation, resolved, json, type => schemas[type]);
    }

    /// <summary>
    /// Synchronous variant for pipelines whose schema generator is synchronous (Swashbuckle): works on both
    /// GetSwagger and GetSwaggerAsync without waiting on asynchronous code.
    /// </summary>
    internal static void ApplyEndpointDeclarations(
        OpenApiOperation operation,
        IEnumerable<object>? metadata,
        JsonSerializerOptions json,
        IServiceProvider? services,
        Func<Type, IOpenApiSchema> schemaFor,
        string endpointName)
    {
        var resolved = ResolveDeclarations(metadata, services, endpointName);
        if (resolved.Count > 0)
            BuildDeclarations(operation, resolved, json, schemaFor);
    }

    private static List<ResolvedDeclaration> ResolveDeclarations(IEnumerable<object>? metadata, IServiceProvider? services, string endpointName)
        => metadata?.OfType<VianaResponseMetadata>().Select(d => Resolve(d, services, endpointName)).ToList() ?? new List<ResolvedDeclaration>();

    private static IEnumerable<Type> RequiredSchemaTypes(List<ResolvedDeclaration> resolved)
    {
        foreach (var group in resolved.GroupBy(r => r.Status))
        {
            var examples = group.Where(r => r.HasExample).ToList();
            if (examples.Any(r => r.Problem != null))
                continue;
            if (examples.Count > 0)
            {
                if (examples[0].WritesBody && examples[0].Body != null)
                    yield return examples[0].Body!.GetType();
                continue;
            }
            var declaration = group.First().Declaration;
            if (!declaration.IsProblem && declaration.DataType != null)
                yield return declaration.DataType;
        }
    }

    private static void BuildDeclarations(OpenApiOperation operation, List<ResolvedDeclaration> resolved, JsonSerializerOptions json, Func<Type, IOpenApiSchema> schemaFor)
    {
        operation.Responses ??= new OpenApiResponses();

        foreach (var group in resolved.GroupBy(r => r.Status))
        {
            var key = group.Key.ToString();
            var examples = group.Where(r => r.HasExample).ToList();
            var problems = examples.Where(r => r.Problem != null).ToList();

            if (problems.Count > 0)
            {
                var schema = new ProblemResultSchema(json, problems[0].Problem!);
                foreach (var other in problems.Skip(1))
                {
                    foreach (var member in other.Problem!.Extensions)
                        schema.AddFieldCore(member.Value?.GetType(), member.Key, "", member.Value);
                }
                var response = schema.BuildCore([ProblemResultSchema.ProblemContentType]);
                operation.Responses[key] = response;
                ApplyExamplesToContent(response.Content!, problems.Select(r => ToBuilt(r, r.Problem, json)).ToList());
                continue;
            }

            if (examples.Count > 0)
            {
                var first = examples[0];
                if (!first.WritesBody)
                {
                    operation.Responses[key] = new OpenApiResponse { Description = Describe(group.Key) };
                    continue;
                }

                var content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = schemaFor(first.Body!.GetType()) }
                };
                operation.Responses[key] = new OpenApiResponse { Description = Describe(group.Key), Content = content };
                ApplyExamplesToContent(content, examples.Select(r => ToBuilt(r, r.Body, json)).ToList());
                continue;
            }

            var declaration = group.First().Declaration;
            if (declaration.IsProblem)
            {
                var schema = new ProblemResultSchema(json, group.Key, declaration.Type, declaration.Title);
                if (!string.IsNullOrEmpty(declaration.Detail))
                    schema.AddFieldCore(JsonSchemaType.String, "detail", "RFC 9457: human-readable explanation specific to this occurrence", declaration.Detail);
                operation.Responses[key] = schema.BuildCore([ProblemResultSchema.ProblemContentType]);
            }
            else if (declaration.DataType != null)
            {
                operation.Responses[key] = new OpenApiResponse
                {
                    Description = Describe(group.Key),
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType { Schema = schemaFor(declaration.DataType) }
                    }
                };
            }
        }
    }

    private static ResolvedDeclaration Resolve(VianaResponseMetadata declaration, IServiceProvider? services, string endpointName)
    {
        object? value = null;
        var hasExample = false;
        if (declaration.InlineExample != null)
        {
            value = declaration.InlineExample;
            hasExample = true;
        }
        else if (declaration.ExampleProviderType != null)
        {
            var provider = ExampleResolver.CreateProvider(declaration.ExampleProviderType, services)
                ?? throw new InvalidOperationException($"Endpoint '{endpointName}': could not create example provider '{declaration.ExampleProviderType.FullName}'.");
            value = ExampleResolver.InvokeGetExample(provider);
            hasExample = true;
        }

        if (value is IResult result)
        {
            if (declaration.Status is int explicitStatus && explicitStatus != result.Status)
                throw new InvalidOperationException(
                    $"Endpoint '{endpointName}': the declared status {explicitStatus} differs from the example result status {result.Status}.");

            var writesBody = ResultResponseBody.HasBody(result);
            var body = ResultResponseBody.GetBody(result);
            if (result.Problem == null && writesBody && body == null)
                return new ResolvedDeclaration(declaration, 204, true, null, null, false); // null value → 204 (runtime rule)

            return new ResolvedDeclaration(declaration, result.Status, true, result.Problem, body, writesBody);
        }

        return new ResolvedDeclaration(declaration, declaration.Status ?? 200, hasExample, null, value, value != null);
    }

    private static BuiltExample ToBuilt(ResolvedDeclaration r, object? value, JsonSerializerOptions json)
    {
        var d = r.Declaration;
        var registration = new ExampleRegistration(r.Status, d.ExampleProviderType ?? typeof(object), d.Name, d.Summary, null);
        var key = d.Name ?? d.ExampleProviderType?.Name ?? "example";
        return new BuiltExample(key, registration, ExampleResolver.Serialize(value, json), r.Problem, r.Status);
    }

    private static string Describe(int status)
        => HttpStatusInfo.TryGetReasonPhrase(status, out var phrase) ? phrase : "Response";

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
