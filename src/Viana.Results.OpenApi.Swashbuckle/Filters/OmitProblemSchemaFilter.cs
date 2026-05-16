using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Schema filter that removes the wrapper-only properties (<c>status</c> and <c>problem</c>)
/// from any schema that implements <see cref="IResult"/>.
/// </summary>
public class OmitProblemSchemaFilter : ISchemaFilter
{
    /// <inheritdoc />
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        => OpenApiSchemaProcessor.OmitProblemFields(schema, context.Type);
}
