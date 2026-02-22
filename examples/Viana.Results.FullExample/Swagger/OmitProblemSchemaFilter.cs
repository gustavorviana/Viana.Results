using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.FullExample.Swagger;

/// <summary>
/// Schema filter for result types:
/// - For <c>Result</c>: removes all properties (no response body for 200).
/// - For <c>Result&lt;T&gt;</c> (when T is not list-like): exposes only the schema of T (the data payload).
/// - For list-like results (<c>ListResult</c>, <c>PagedResult</c>, etc.): removes only the <c>problem</c> field.
/// 
/// This filter primarily works against <see cref="IOpenApiSchema"/> and only casts to
/// <see cref="OpenApiSchema"/> when mutation of the schema object is required.
/// </summary>
public class OmitProblemSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is null)
            return;

        var type = context.Type;

        if (!typeof(IResult).IsAssignableFrom(type))
            return;

        if (type == typeof(Result))
        {
            if (schema is OpenApiSchema mutable)
                MakeSchemaEmpty(mutable);
            return;
        }

        schema.Properties?.Remove("status");
        schema.Properties?.Remove("problem");
    }

    private static void MakeSchemaEmpty(OpenApiSchema schema)
    {
        schema.Type = null;
        schema.Format = null;
        schema.Description = null;
        schema.Properties = null;
        schema.Required = null;
        schema.Items = null;
        schema.AdditionalProperties = null;
        schema.AdditionalPropertiesAllowed = false;
        schema.AllOf = null;
        schema.AnyOf = null;
        schema.OneOf = null;
    }
}