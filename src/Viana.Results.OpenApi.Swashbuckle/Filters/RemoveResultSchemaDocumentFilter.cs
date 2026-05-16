using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Document filter that removes the bare <c>Result</c> schema from the components section.
/// </summary>
public sealed class RemoveResultSchemaDocumentFilter : IDocumentFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        => OpenApiDocumentProcessor.RemoveBareResultSchema(swaggerDoc);
}
