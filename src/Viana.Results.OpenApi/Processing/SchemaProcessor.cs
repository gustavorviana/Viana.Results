using Microsoft.OpenApi;
using System;

namespace Viana.Results.OpenApi.Processing;

/// <summary>
/// Shared schema-level mutations applied to <see cref="OpenApiSchema"/> instances.
/// Used by both the Swashbuckle <c>ISchemaFilter</c> and the <c>Microsoft.AspNetCore.OpenApi</c>
/// <c>IOpenApiSchemaTransformer</c> implementations.
/// </summary>
internal static class SchemaProcessor
{
    /// <summary>
    /// Removes the wrapper-only <c>status</c> and <c>problem</c> properties from any schema
    /// whose CLR type implements <see cref="IResult"/>. The payload unwrap is performed elsewhere.
    /// </summary>
    internal static void OmitProblemFields(IOpenApiSchema schema, Type type)
    {
        if (schema is null || type is null)
            return;

        if (!typeof(IResult).IsAssignableFrom(type))
            return;

        schema.Properties?.Remove("status");
        schema.Properties?.Remove("problem");

        // Feature 014: an envelope with extensions writes them at the root, not as a member.
        if (ResultReflections.IsEnvelopeType(type) && typeof(IHasExtensions).IsAssignableFrom(type))
        {
            schema.Properties?.Remove("extensions");
            if (schema is OpenApiSchema concrete)
                concrete.AdditionalProperties = new OpenApiSchema();
        }
    }
}
