using Microsoft.OpenApi;

namespace Viana.Results.OpenApi.Processing;

/// <summary>
/// Shared document-level mutations applied to <see cref="OpenApiDocument"/>.
/// Used by both Swashbuckle <c>IDocumentFilter</c> and <c>Microsoft.AspNetCore.OpenApi</c>
/// <c>IOpenApiDocumentTransformer</c> implementations.
/// </summary>
public static class OpenApiDocumentProcessor
{
    /// <summary>
    /// Removes the bare <c>Result</c> schema from the components section (it is never used
    /// as a response body — only error and unwrapped payloads appear in operations).
    /// </summary>
    public static void RemoveBareResultSchema(OpenApiDocument document)
    {
        document.Components?.Schemas?.Remove("Result");
    }
}
