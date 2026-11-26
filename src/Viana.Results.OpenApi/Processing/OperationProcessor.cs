using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Http.Metadata;
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
    /// Feature 011: documents the success of an endpoint that returns a Viana result.
    /// <list type="bullet">
    /// <item>Successes declared by hand win: <c>[ProducesResponseType]</c> / <c>Produces</c> (with no type, void or a
    /// non-Viana type), <see cref="ResponseExampleAttribute"/> and <c>ProducesResult</c> (status from the example result).</item>
    /// <item>The success inferred from the return type (ASP.NET Core's 200) is removed when another success is declared,
    /// for a bare <see cref="Result"/>, or with <see cref="WithoutDefaultResultAttribute"/>.</item>
    /// <item>A bare <see cref="Result"/> with nothing declared documents <c>204 No Content</c> (what <c>Results.Ok()</c> answers).</item>
    /// <item><see cref="WithoutDefaultResultAttribute"/>: no inferred success at all; only the hand-registered ones.</item>
    /// </list>
    /// Runs before the problem and example steps; they add their responses afterwards.
    /// </summary>
    /// <remarks>
    /// <c>Produces&lt;Result&gt;(200)</c> cannot be told apart from the inferred 200 (same type); declare
    /// <c>Produces(200)</c> or <c>ProducesResult</c> instead.
    /// </remarks>
    internal static void ApplyDefaultResult(OpenApiOperation operation, MethodInfo method, IReadOnlyList<object>? endpointMetadata, IServiceProvider? services, string endpointName)
    {
        var returnType = UnwrapTaskType(method.ReturnType);
        if (!typeof(IResult).IsAssignableFrom(returnType))
            return;

        operation.Responses ??= new OpenApiResponses();
        var metadata = endpointMetadata ?? Array.Empty<object>();
        var isBare = returnType == typeof(Result);
        var without = metadata.OfType<WithoutDefaultResultAttribute>().Any()
            || method.GetCustomAttribute<WithoutDefaultResultAttribute>(true) != null
            || method.DeclaringType?.GetCustomAttribute<WithoutDefaultResultAttribute>(true) != null;

        var declared = DeclaredSuccesses(method, metadata, services, endpointName);

        // The response ASP.NET Core inferred from the return type (schema of T for Result<T>).
        var inferredKeys = operation.Responses.Keys.Where(k => IsSuccess(k) && !declared.Statuses.Contains(int.Parse(k))).ToList();
        var template = inferredKeys.Select(k => operation.Responses[k]).FirstOrDefault(r => r.Content is { Count: > 0 });

        if (without || isBare || declared.Statuses.Count > 0)
        {
            foreach (var key in inferredKeys)
                operation.Responses.Remove(key);
        }

        // Attribute examples only decorate existing responses: give their statuses the inferred response shape
        // (e.g. [ResponseExample] of a Created result on Result<T> documents 201 with T).
        foreach (var (status, writesBody) in declared.AttributeExamples)
        {
            var key = status.ToString();
            if (operation.Responses.ContainsKey(key))
                continue;
            operation.Responses[key] = writesBody && template != null && !isBare
                ? Clone(template, Describe(status))
                : new OpenApiResponse { Description = Describe(status) };
        }

        if (isBare && !without && declared.Statuses.Count == 0)
            operation.Responses["204"] = new OpenApiResponse { Description = Describe(204) };

        if (!isBare)
            return;

        // A bare Result has no body: drop the content ApiExplorer derived from the return type, unless the
        // success was declared with its own type ([ProducesResponseType<T>]).
        foreach (var pair in operation.Responses)
        {
            if (IsSuccess(pair.Key) && !declared.Typed.Contains(int.Parse(pair.Key)) && pair.Value.Content is { Count: > 0 } content)
                content.Clear();
        }
    }

    private sealed record DeclaredSuccess(HashSet<int> Statuses, HashSet<int> Typed, List<(int Status, bool WritesBody)> AttributeExamples);

    private static DeclaredSuccess DeclaredSuccesses(MethodInfo method, IReadOnlyList<object> metadata, IServiceProvider? services, string endpointName)
    {
        var statuses = new HashSet<int>();
        var typed = new HashSet<int>();
        var attributeExamples = new List<(int, bool)>();

        // [ProducesResponseType] / Produces(...): the inferred metadata carries the Viana return type itself.
        var produces = metadata.OfType<IProducesResponseTypeMetadata>()
            .Concat(method.GetCustomAttributes(true).OfType<IProducesResponseTypeMetadata>())
            .Concat(method.DeclaringType?.GetCustomAttributes(true).OfType<IProducesResponseTypeMetadata>() ?? []);
        // MVC's [ProducesResponseType] exposes its status through IApiResponseMetadataProvider.
        var mvc = method.GetCustomAttributes(true).OfType<IApiResponseMetadataProvider>()
            .Concat(method.DeclaringType?.GetCustomAttributes(true).OfType<IApiResponseMetadataProvider>() ?? [])
            .Select(p => (p.StatusCode, p.Type));
        foreach (var (status, type) in produces.Select(p => (p.StatusCode, p.Type)).Concat(mvc))
        {
            if (!IsSuccess(status) || (type != null && typeof(IResult).IsAssignableFrom(type)))
                continue;
            statuses.Add(status);
            if (type != null && type != typeof(void))
                typed.Add(status);
        }

        // [ResponseExample] on the action or its type (globals are fallbacks, not declarations).
        var byStatus = ExampleResolver.Collect(method, Array.Empty<GlobalExampleRegistration>());
        foreach (var example in byStatus.Values.SelectMany(list => BuildExamples(list, ExampleJson, services, method)))
        {
            if (example.Problem != null || !IsSuccess(example.Status))
                continue;
            statuses.Add(example.Status);
            attributeExamples.Add((example.Status, example.Node != null));
        }

        // ProducesResult (minimal APIs): status from the example result, or the declared/default status.
        foreach (var declaration in metadata.OfType<VianaResponseMetadata>().Where(d => !d.IsProblem))
        {
            var status = Resolve(declaration, services, endpointName).Status;
            if (IsSuccess(status))
                statuses.Add(status);
        }

        return new DeclaredSuccess(statuses, typed, attributeExamples);
    }

    private static readonly JsonSerializerOptions ExampleJson = new(JsonSerializerDefaults.Web);

    private static bool IsSuccess(int status) => status is >= 200 and < 300;

    private static bool IsSuccess(string key) => int.TryParse(key, out var status) && IsSuccess(status);

    private static OpenApiResponse Clone(IOpenApiResponse template, string description) => new()
    {
        Description = description,
        Content = template.Content?.ToDictionary(c => c.Key, c => new OpenApiMediaType { Schema = c.Value.Schema })
    };

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

        // Feature 012: a ProblemResult example is a problem with its own status (as on controllers), whichever
        // overload declared it.
        if (value is ProblemResult problem)
        {
            if (declaration.Status is int explicitProblemStatus && explicitProblemStatus != problem.Status)
                throw new InvalidOperationException(
                    $"Endpoint '{endpointName}': the declared status {explicitProblemStatus} differs from the example problem status {problem.Status}.");

            return new ResolvedDeclaration(declaration, problem.Status, true, problem, problem, true);
        }

        if (declaration.IsProblem && hasExample && value != null)
            throw new InvalidOperationException(
                $"Endpoint '{endpointName}': problem example provider '{declaration.ExampleProviderType?.Name ?? value.GetType().Name}' must return a Viana result or a ProblemResult.");

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
