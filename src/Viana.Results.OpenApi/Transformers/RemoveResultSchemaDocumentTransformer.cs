using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI document transformer that removes the bare <c>Result</c> schema from components.
/// </summary>
public sealed class RemoveResultSchemaDocumentTransformer : IOpenApiDocumentTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        OpenApiDocumentProcessor.RemoveBareResultSchema(document);
        return Task.CompletedTask;
    }
}
