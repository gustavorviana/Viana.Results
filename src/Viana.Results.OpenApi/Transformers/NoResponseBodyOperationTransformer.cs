using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI operation transformer that removes response bodies when the action returns <see cref="Result"/>.
/// </summary>
public sealed class NoResponseBodyOperationTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var method = TransformerHelper.GetMethodInfo(context.Description);
        if (method != null)
            OpenApiOperationProcessor.ClearBodyForBareResult(operation, method);
        return Task.CompletedTask;
    }
}
