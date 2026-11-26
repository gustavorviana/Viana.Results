using Microsoft.OpenApi;
using System;

namespace Viana.Results.OpenApi.Processing;

/// <summary>Helper shared by the native OpenAPI transformers and the Swashbuckle filters.</summary>
[Obsolete("Internal helper; not meant for direct use.")]
public static class OpenApiSchemaProcessor
{
    /// <summary>See the native transformer / Swashbuckle filter that uses it.</summary>
    public static void OmitProblemFields(IOpenApiSchema schema, Type type) => SchemaProcessor.OmitProblemFields(schema, type);
}
