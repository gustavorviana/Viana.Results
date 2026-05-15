using Microsoft.AspNetCore.OpenApi;
using Viana.Results.OpenApi.Transformers;

namespace Viana.Results.OpenApi;

/// <summary>
/// Extension methods to register Viana.Results OpenAPI transformers in one call.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Registers all Viana.Results OpenAPI transformers into the current <see cref="OpenApiOptions"/> pipeline.
    /// </summary>
    /// <param name="options">The OpenAPI options.</param>
    /// <returns>The same <see cref="OpenApiOptions"/> instance for chaining.</returns>
    public static OpenApiOptions AddVianaResultTransformers(this OpenApiOptions options)
    {
        options.AddSchemaTransformer<OmitProblemSchemaTransformer>();
        options.AddOperationTransformer<NoResponseBodyOperationTransformer>();
        options.AddDocumentTransformer<RemoveResultSchemaDocumentTransformer>();
        options.AddOperationTransformer<ProblemResponseOperationTransformer>();
        options.AddSchemaTransformer<UnwrapResultSchemaTransformer>();
        options.AddOperationTransformer<UnauthorizedAndForbiddenOperationTransformer>();
        options.AddOperationTransformer<ResponseExampleOperationTransformer>();
        return options;
    }
}
