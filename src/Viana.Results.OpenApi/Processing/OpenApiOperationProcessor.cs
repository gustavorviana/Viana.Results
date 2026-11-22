using Microsoft.OpenApi;
using System;
using System.Reflection;
using System.Text.Json;

namespace Viana.Results.OpenApi.Processing;

/// <summary>
/// Operation-level helpers shared by the native OpenAPI transformers and the Swashbuckle filters.
/// </summary>
[Obsolete("Internal helper; it will become internal in Viana.Results.OpenApi 2.0.0.")]
public static class OpenApiOperationProcessor
{
    /// <summary>Clears the response body for actions returning a bare <see cref="Result"/>.</summary>
    public static void ClearBodyForBareResult(OpenApiOperation operation, MethodInfo method)
        => OperationProcessor.ClearBodyForBareResult(operation, method);

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
