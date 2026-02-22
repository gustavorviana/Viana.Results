using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.FullExample.Swagger;

/// <summary>
/// Adds standardized error responses (400 and 401) using an RFC 9457-compatible
/// <c>problem</c> schema to operations that return <see cref="IResult"/>.
/// 
/// The filter unwraps <c>Task&lt;T&gt;</c> return types and only applies when the
/// effective return type implements <see cref="IResult"/>. When enabled, each
/// response is documented with an <c>application/json</c> payload that follows
/// the Problem Details structure.
/// </summary>
public class ProblemResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var returnType = context.MethodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name == "Task`1")
            returnType = returnType.GetGenericArguments()[0];

        if (!typeof(IResult).IsAssignableFrom(returnType))
            return;

        //var problemSchema400 = CreateProblemSchema(
        //    "RFC 9457: problem type URI",
        //    "RFC 9457: short, human-readable summary",
        //    "RFC 9457: HTTP status code",
        //    "RFC 9457: optional extension members (e.g. validation errors)");

        //operation.Responses!["400"] = new OpenApiResponse
        //{
        //    Description = "Bad Request",
        //    Content = new Dictionary<string, OpenApiMediaType>
        //    {
        //        ["application/json"] = new OpenApiMediaType
        //        {
        //            Schema = problemSchema400,
        //            Example = JsonNode.Parse("""{"type":"about:blank","title":"Bad Request","status":400,"extensions":{}}""")
        //        }
        //    }
        //};

        //var problemSchema401 = CreateProblemSchema(null, null, null, null);

        //operation.Responses["401"] = new OpenApiResponse
        //{
        //    Description = "Unauthorized",
        //    Content = new Dictionary<string, OpenApiMediaType>
        //    {
        //        ["application/json"] = new OpenApiMediaType
        //        {
        //            Schema = problemSchema401,
        //            Example = JsonNode.Parse("""{"type":"about:blank","title":"Unauthorized","status":401,"extensions":{}}""")
        //        }
        //    }
        //};
    }

    private static OpenApiSchema CreateProblemSchema(string? typeDesc, string? titleDesc, string? statusDesc, string? extensionsDesc)
    {
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = typeDesc },
                ["title"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = titleDesc },
                ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Description = statusDesc },
                ["extensions"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Description = extensionsDesc,
                    AdditionalPropertiesAllowed = true
                }
            }
        };
        return schema;
    }
}
