using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI schema transformer for result types:
/// removes <c>status</c> and <c>problem</c> properties from schemas that implement <see cref="IResult"/>.
/// </summary>
public sealed class OmitProblemSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema is null)
            return Task.CompletedTask;

        var type = context.JsonTypeInfo.Type;

        if (!typeof(IResult).IsAssignableFrom(type))
            return Task.CompletedTask;

        schema.Properties?.Remove("status");
        schema.Properties?.Remove("problem");

        return Task.CompletedTask;
    }
}
