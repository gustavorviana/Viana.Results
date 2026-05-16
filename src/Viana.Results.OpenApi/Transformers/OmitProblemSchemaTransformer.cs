using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI schema transformer that removes <c>status</c> and <c>problem</c> from result wrappers.
/// </summary>
public sealed class OmitProblemSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        OpenApiSchemaProcessor.OmitProblemFields(schema, context.JsonTypeInfo.Type);
        return Task.CompletedTask;
    }
}
