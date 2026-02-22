#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#endif

namespace Viana.Results.OpenApi.Swashbuckle.Schemas;

public record ProblemFieldSchema(JsonSchemaType Type, string Name, string Description, object? ExampleValue = null)
{
    public OpenApiSchema ToOpenApi()
    {
        return new OpenApiSchema { Type = Type, Description = Description };
    }

    public string GetExampleJsonField()
    {
        return $"\"{Name}\":\"{GetExampleJsonValue()}\"";
    }

    public string GetExampleJsonValue()
    {
        if (ExampleValue == null)
            return "null";

        if (ExampleValue is string strVal)
            return strVal.Replace("\"", "\"\"");

        return ExampleValue.ToString() ?? "null";
    }
}