#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif

namespace Viana.Results.OpenApi.Swashbuckle.Schemas;

/// <summary>
/// Represents a single field definition used to compose an RFC 9457 problem schema
/// for OpenAPI documentation.
/// </summary>
/// <param name="Type">The JSON schema type of the field.</param>
/// <param name="Name">The field name.</param>
/// <param name="Description">The human-readable description of the field.</param>
/// <param name="ExampleValue">Optional example value used in the generated example payload.</param>
public record ProblemFieldSchema(JsonSchemaType Type, string Name, string Description, object? ExampleValue = null)
{
    /// <summary>
    /// Converts this field definition into an <see cref="OpenApiSchema"/> instance.
    /// </summary>
    /// <returns>An OpenAPI schema representing this field.</returns>
    public OpenApiSchema ToOpenApi()
    {
        return new OpenApiSchema
        {
#if NET10_0_OR_GREATER
            Type = Type,
#else
            Type = Type.ToString().ToLowerInvariant(),
#endif
            Description = Description
        };
    }

    /// <summary>
    /// Builds the JSON fragment used in the example payload for this field.
    /// </summary>
    /// <returns>A JSON property string representing the example field.</returns>
    public string GetExampleJsonField()
    {
        return $"\"{Name}\":\"{GetExampleJsonValue()}\"";
    }

    /// <summary>
    /// Gets the formatted example value for JSON output.
    /// </summary>
    /// <returns>A string representation of the example value.</returns>
    public string GetExampleJsonValue()
    {
        if (ExampleValue == null)
            return "null";

        if (ExampleValue is string strVal)
            return strVal.Replace("\"", "\"\"");

        return ExampleValue.ToString() ?? "null";
    }
}