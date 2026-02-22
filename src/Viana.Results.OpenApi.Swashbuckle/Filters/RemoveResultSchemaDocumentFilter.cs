using Microsoft.OpenApi;
#if NET8_0
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

public sealed class RemoveResultSchemaDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        => swaggerDoc.Components?.Schemas?.Remove("Result");
}
