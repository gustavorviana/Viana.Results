using Microsoft.OpenApi;
#if NET8_0
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Swagger document filter that removes the <c>Result</c> schema from the
/// generated OpenAPI components section.
/// </summary>
public sealed class RemoveResultSchemaDocumentFilter : IDocumentFilter
{
    /// <summary>
    /// Applies the filter to the specified OpenAPI document.
    /// </summary>
    /// <param name="swaggerDoc">The OpenAPI document being processed.</param>
    /// <param name="context">The document filter context.</param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        => swaggerDoc.Components?.Schemas?.Remove("Result");
}