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
public static class OpenApiOperationProcessor
{
    /// <summary>
    /// When the action returns the non-generic <see cref="Result"/>, clears all response
    /// content (Result has no body — only the HTTP status code).
    /// </summary>
    public static void ClearBodyForBareResult(OpenApiOperation operation, MethodInfo method)
    {
        if (operation?.Responses is null || operation.Responses.Count == 0)
            return;

        var returnType = method.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name == "Task`1")
            returnType = returnType.GetGenericArguments()[0];

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
    public static void ApplyProblemResultAttributes(OpenApiOperation operation, MethodInfo method, JsonSerializerOptions json)
    {
        var returnType = method.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name == "Task`1")
            returnType = returnType.GetGenericArguments()[0];

        if (!typeof(IResult).IsAssignableFrom(returnType))
            return;

        foreach (var problem in GetProblemAttributes(method))
            ProblemResultSchema.FromAttribute(json, problem).ApplyTo(operation.Responses);
    }

    /// <summary>
    /// When the action (or its declaring type) is guarded by <c>[Authorize]</c>, registers
    /// standard 401 and 403 RFC 9457 problem responses.
    /// </summary>
    public static void ApplyAuthorizeResponses(OpenApiOperation operation, MethodInfo method, JsonSerializerOptions json)
    {
        var hasAuthorize = method.GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any()
            || method.DeclaringType?.GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any() == true;

        if (!hasAuthorize)
            return;

        new ProblemResultSchema(json, 401).ApplyTo(operation.Responses);
        new ProblemResultSchema(json, 403).ApplyTo(operation.Responses);
    }

    /// <summary>
    /// Materializes <see cref="ResponseExampleAttribute"/> declarations (and globals from
    /// <see cref="VianaResultOptions"/>) into <c>example</c>/<c>examples</c> on each JSON
    /// response media type.
    /// </summary>
    public static void ApplyResponseExamples(
        OpenApiOperation operation,
        MethodInfo method,
        JsonSerializerOptions json,
        IServiceProvider? services,
        VianaResultOptions? options)
    {
        if (operation.Responses == null || operation.Responses.Count == 0)
            return;

        var globals = options?.GlobalExamples ?? Array.Empty<GlobalExampleRegistration>();
        var byStatus = ResponseExampleResolver.Collect(method, globals);
        if (byStatus.Count == 0)
            return;

        foreach (var pair in byStatus)
        {
            var statusKey = pair.Key.ToString();
            if (!operation.Responses.TryGetValue(statusKey, out var response) || response.Content == null)
                continue;

            ApplyExamplesToResponse(response, pair.Value, json, services);
        }
    }

    private static void ApplyExamplesToResponse(IOpenApiResponse response, List<ResolvedExample> examples, JsonSerializerOptions json, IServiceProvider? services)
    {
        var built = new List<(string Key, ResolvedExample Meta, System.Text.Json.Nodes.JsonNode? Node)>();

        foreach (var ex in examples)
        {
            var provider = ResponseExampleResolver.CreateProvider(ex.ProviderType, services);
            if (provider == null)
                continue;

            var value = ResponseExampleResolver.InvokeGetExample(provider);
            var node = ResponseExampleResolver.Serialize(value, json);
            var key = ex.Name ?? ex.ProviderType.Name;
            built.Add((key, ex, node));
        }

        if (built.Count == 0)
            return;

        var useExamplesMap = built.Count > 1 || built.Any(b => b.Meta.Name != null);

        foreach (var media in response.Content!.Values)
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
