using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.FullExample.Swagger;

public sealed class RemoveResultSchemaDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Components?.Schemas is null)
            return;

        swaggerDoc.Components.Schemas.Remove("Result");
    }
}