using System.Collections.Generic;
using Microsoft.OpenApi;
using System;
using System.Reflection;
using System.Text.Json;

namespace Viana.Results.OpenApi.Processing;

/// <summary>
/// Operation-level helpers shared by the native OpenAPI transformers and the Swashbuckle filters.
/// </summary>
[Obsolete("Internal helper; not meant for direct use.")]
public static class OpenApiOperationProcessor
{
    /// <summary>
    /// Documents the success of an endpoint returning a Viana result: a bare <see cref="Result"/> is 204, hand-declared
    /// successes win over the inferred one, and <see cref="WithoutDefaultResultAttribute"/> keeps only the declared ones.
    /// </summary>
    public static void ApplyDefaultResult(OpenApiOperation operation, MethodInfo method, IReadOnlyList<object>? endpointMetadata, IServiceProvider? services = null)
        => OperationProcessor.ApplyDefaultResult(operation, method, endpointMetadata, services, method.Name);

    /// <summary>Adds RFC 9457 problem responses for every <see cref="ProblemResultAttribute"/> on the action or its type.</summary>
    public static void ApplyProblemResultAttributes(OpenApiOperation operation, MethodInfo method, JsonSerializerOptions json)
        => OperationProcessor.ApplyProblemResultAttributes(operation, method, json);

    /// <summary>Adds 401/403 problem responses when the action requires authorization.</summary>
    public static void ApplyAuthorizeResponses(OpenApiOperation operation, MethodInfo method, JsonSerializerOptions json)
        => OperationProcessor.ApplyAuthorizeResponses(operation, null, method, json);

    /// <summary>Materializes response examples into each JSON response media type.</summary>
    public static void ApplyResponseExamples(OpenApiOperation operation, MethodInfo method, JsonSerializerOptions json, IServiceProvider? services, VianaResultOptions? options)
        => OperationProcessor.ApplyResponseExamples(operation, method, json, services, options);
}
