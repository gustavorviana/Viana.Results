using Microsoft.OpenApi;

namespace Viana.Results.OpenApi.Schemas;

// Internal field definition used by ProblemResultSchema.
internal sealed record ProblemField(JsonSchemaType Type, string Name, string Description, object? ExampleValue = null)
{
    public OpenApiSchema ToOpenApi() => new() { Type = Type, Description = Description };
}
