using System.Reflection;
using System.Text.Json;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Helpers to run the processors on one operation, the way the pipelines do.</summary>
internal static class Ops
{
    public static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static OpenApiOperation NewOperation() => new()
    {
        Responses = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse
            {
                Description = "OK",
                Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new() { Schema = new OpenApiSchema() } }
            }
        }
    };

    public static MethodInfo Method<T>(string name) => typeof(T).GetMethod(name)!;

    public static EndpointScope Controller<T>(ListLogger? logger = null) => new(typeof(T), logger);

    public static EndpointScope Minimal => new(null, null);

    /// <summary>Every controller step, in pipeline order.</summary>
    public static OpenApiOperation RunController<T>(string action, ListLogger? logger = null)
    {
        var operation = NewOperation();
        var method = Method<T>(action);
        var scope = Controller<T>(logger);
        OperationProcessor.ApplyDefaultResult(operation, method, null, null, action, scope);
        OperationProcessor.ApplyProblemResultAttributes(operation, method, Json, scope);
        OperationProcessor.ApplyResponseExamples(operation, method, Json, null, null, scope);
        return operation;
    }

    /// <summary>Every minimal API step, in pipeline order.</summary>
    public static OpenApiOperation RunMinimal(MethodInfo handler, IReadOnlyList<object> metadata, VianaResultOptions? options = null)
    {
        var operation = NewOperation();
        OperationProcessor.ApplyDefaultResult(operation, handler, metadata, null, "GET /x", Minimal);
        OperationProcessor.ApplyProblemResultAttributes(operation, handler, Json, Minimal);
        OperationProcessor.ApplyResponseExamples(operation, handler, Json, null, options, Minimal);
        OperationProcessor.ApplyEndpointDeclarations(operation, metadata, Json, null, _ => new OpenApiSchema(), "GET /x", handler, Minimal);
        return operation;
    }
}
