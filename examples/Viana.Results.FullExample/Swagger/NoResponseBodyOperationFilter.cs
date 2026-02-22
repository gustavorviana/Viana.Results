using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.FullExample.Swagger;

public sealed class NoResponseBodyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null)
            return;
            
        foreach (var kv in operation.Responses)
        {
            var response = kv.Value;

            if (!HasOnlyYourResultSchema(response) || response.Content is null)
                continue;

            response.Content.Clear();
            response.Description ??= "OK";
        }
    }

    private static bool HasOnlyYourResultSchema(IOpenApiResponse response)
    {
        if (response.Content is null || response.Content.Count == 0)
            return false;

        foreach (var media in response.Content.Values)
        {
            var schema = media.Schema;
            if (schema is null)
                continue;

            if (schema is OpenApiSchemaReference referenceable)
            {
                var reference = referenceable.Reference;
                var refId = reference?.Id;

                if (string.Equals(refId, "Result", StringComparison.Ordinal))
                    return true;
            }

            if (string.Equals(schema.Title, "Result", StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}