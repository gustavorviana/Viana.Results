using Microsoft.OpenApi;
using System;

namespace Viana.Results.OpenApi.Processing;

/// <summary>
/// Shared schema-level mutations applied to <see cref="OpenApiSchema"/> instances.
/// Used by both the Swashbuckle <c>ISchemaFilter</c> and the <c>Microsoft.AspNetCore.OpenApi</c>
/// <c>IOpenApiSchemaTransformer</c> implementations.
/// </summary>
public static class OpenApiSchemaProcessor
{
    /// <summary>
    /// Removes the wrapper-only <c>status</c> and <c>problem</c> properties from any schema
    /// whose CLR type implements <see cref="IResult"/>. The payload unwrap is performed elsewhere.
    /// </summary>
    public static void OmitProblemFields(IOpenApiSchema schema, Type type)
    {
        if (schema is null || type is null)
            return;

        if (!typeof(IResult).IsAssignableFrom(type))
            return;

        schema.Properties?.Remove("status");
        schema.Properties?.Remove("problem");
    }
}
