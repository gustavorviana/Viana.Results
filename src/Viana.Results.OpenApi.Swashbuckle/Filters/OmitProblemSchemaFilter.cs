using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Schema filter for result types: removes the wrapper-only properties (<c>status</c> and <c>problem</c>)
/// from any schema that implements <see cref="IResult"/>. The actual payload unwrapping
/// (for <c>Result&lt;T&gt;</c> / <c>ListResult&lt;T&gt;</c>) is performed by <see cref="UnwrapResultFilter"/>.
/// </summary>
public class OmitProblemSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Applies the schema transformation rules for Viana result types.
    /// </summary>
    /// <param name="schema">The OpenAPI schema being processed.</param>
    /// <param name="context">The schema filter context.</param>
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is null)
            return;

        var type = context.Type;

        if (!typeof(IResult).IsAssignableFrom(type))
            return;

        schema.Properties?.Remove("status");
        schema.Properties?.Remove("problem");
    }
}
