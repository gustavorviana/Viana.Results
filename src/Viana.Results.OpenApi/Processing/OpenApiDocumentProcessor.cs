using Microsoft.OpenApi;

namespace Viana.Results.OpenApi.Processing;

/// <summary>Helper shared by the native OpenAPI transformers and the Swashbuckle filters.</summary>
[System.Obsolete("Internal helper; it will become internal in Viana.Results.OpenApi 2.0.0.")]
public static class OpenApiDocumentProcessor
{
    /// <summary>See the native transformer / Swashbuckle filter that uses it.</summary>
    public static void RemoveBareResultSchema(OpenApiDocument document) => DocumentProcessor.RemoveBareResultSchema(document);
}
