using Microsoft.OpenApi;
#if NET8_0
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Schema filter for result types:
/// - For <c>Result</c>: removes all properties (no response body for 200).
/// - For <c>Result&lt;T&gt;</c> (when T is not list-like): exposes only the schema of T (the data payload).
/// - For list-like results (<c>ListResult</c>, <c>PagedResult</c>, etc.): removes only the <c>problem</c> field.
/// </summary>
public class OmitProblemSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Applies the schema transformation rules for Viana result types.
    /// </summary>
    /// <param name="schema">The OpenAPI schema being processed.</param>
    /// <param name="context">The schema filter context.</param>
#if NET10_0_OR_GREATER
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
#else
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
#endif
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