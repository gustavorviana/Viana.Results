using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.OpenApi;

namespace Viana.Results.OpenApi.Schemas;

// Remembers which inline schemas are Viana problem schemas (and their extra members) so the document
// step can replace them with the shared component, without writing any marker into the document.
internal static class ProblemSchemaRegistry
{
    private static readonly ConditionalWeakTable<IOpenApiSchema, Dictionary<string, IOpenApiSchema>> Extras = new();

    internal static void Register(IOpenApiSchema schema, Dictionary<string, IOpenApiSchema> extras) => Extras.AddOrUpdate(schema, extras);

    internal static bool TryGetExtras(IOpenApiSchema? schema, out Dictionary<string, IOpenApiSchema> extras)
    {
        extras = null!;
        return schema != null && Extras.TryGetValue(schema, out extras!);
    }

    internal static readonly string[] BaseMembers = ["type", "title", "status", "detail", "instance"];

    // Same members as the native ProblemDetails schema: all optional and nullable, no traceId.
    internal static OpenApiSchema CreateBaseSchema() => new()
    {
        Type = JsonSchemaType.Object,
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Description = "RFC 9457: problem type URI" },
            ["title"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Description = "RFC 9457: short, human-readable summary" },
            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer | JsonSchemaType.Null, Format = "int32", Description = "RFC 9457: HTTP status code" },
            ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Description = "RFC 9457: human-readable explanation specific to this occurrence" },
            ["instance"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Description = "RFC 9457: URI reference identifying this occurrence" },
        }
    };
}
