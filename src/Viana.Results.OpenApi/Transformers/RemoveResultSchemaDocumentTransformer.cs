using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI document transformer that removes the <c>Result</c> schema from the
/// generated OpenAPI components section.
/// </summary>
public sealed class RemoveResultSchemaDocumentTransformer : IOpenApiDocumentTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components?.Schemas?.Remove("Result");
        return Task.CompletedTask;
    }
}
