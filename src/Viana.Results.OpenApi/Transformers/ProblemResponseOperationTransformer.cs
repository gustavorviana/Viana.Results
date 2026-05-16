using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI operation transformer that adds RFC 9457 problem response examples
/// to operations decorated with <see cref="ProblemResultAttribute"/>.
/// </summary>
public sealed class ProblemResponseOperationTransformer : IOpenApiOperationTransformer
{
    private readonly IOptions<JsonOptions> _httpJson;

    /// <summary>Initializes the transformer.</summary>
    public ProblemResponseOperationTransformer(IOptions<JsonOptions> httpJson)
    {
        _httpJson = httpJson;
    }

    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var method = TransformerHelper.GetMethodInfo(context.Description);
        if (method != null)
            OpenApiOperationProcessor.ApplyProblemResultAttributes(operation, method, _httpJson.Value.SerializerOptions);
        return Task.CompletedTask;
    }
}
