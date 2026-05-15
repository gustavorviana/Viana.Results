using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.OpenApi.Schemas;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI operation transformer that adds RFC 9457 problem response examples
/// to operations decorated with <see cref="ProblemResultAttribute"/>.
/// </summary>
public sealed class ProblemResponseOperationTransformer : IOpenApiOperationTransformer
{
    private readonly IOptions<JsonOptions> _httpJson;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemResponseOperationTransformer"/> class.
    /// </summary>
    /// <param name="httpJson">Provides access to the configured JSON serializer options.</param>
    public ProblemResponseOperationTransformer(IOptions<JsonOptions> httpJson)
    {
        _httpJson = httpJson;
    }

    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var methodInfo = TransformerHelper.GetMethodInfo(context.Description);
        if (methodInfo == null)
            return Task.CompletedTask;

        var returnType = methodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name == "Task`1")
            returnType = returnType.GetGenericArguments()[0];

        if (!typeof(IResult).IsAssignableFrom(returnType))
            return Task.CompletedTask;

        foreach (var problem in GetProblemAttributes(methodInfo))
            ProblemResultSchema.FromAttribute(_httpJson.Value.SerializerOptions, problem)
                .ApplyTo(operation.Responses);

        return Task.CompletedTask;
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
