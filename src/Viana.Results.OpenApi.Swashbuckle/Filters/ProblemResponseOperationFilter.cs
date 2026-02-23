using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
#if NET8_0
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Reflection;
using Viana.Results.OpenApi.Swashbuckle.Schemas;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Adds response examples to operations that return <see cref="IResult"/>.
/// Each configured example is an <see cref="IResult"/>; the same logic as the MVC pipeline
/// (<see cref="ResultResponseBody.GetBody"/>) is used to determine the response body,
/// and that body is used to generate the schema and JSON example in Swagger.
/// Supports global examples, path-specific examples (<see cref="ProblemResponseOptions.ForPath"/>),
/// <see cref="ProblemResultAttribute"/> per action, and optional 401/403 when auth is required.
/// </summary>
internal class ProblemResponseOperationFilter : IOperationFilter
{
    private readonly IOptions<JsonOptions> _httpJson;

    public ProblemResponseOperationFilter(IOptions<JsonOptions> httpJson)
    {
        _httpJson = httpJson;
    }

#if NET10_0_OR_GREATER
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
#else
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
#endif
    {
        var returnType = context.MethodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name == "Task`1")
            returnType = returnType.GetGenericArguments()[0];

        if (!typeof(IResult).IsAssignableFrom(returnType))
            return;

        foreach (var problem in GetProblemAttributes(context.MethodInfo))
            ProblemResultSchema.FromAttribute(_httpJson.Value.SerializerOptions, problem)
                .ApplyTo(operation.Responses);
    }

    private static List<ProblemResultAttribute> GetProblemAttributes(MethodInfo method)
    {
        var attributes = new List<ProblemResultAttribute>();

        attributes.AddRange(method.GetCustomAttributes<ProblemResultAttribute>(true));
        if (method.DeclaringType != null)
            attributes.AddRange(method.DeclaringType.GetCustomAttributes<ProblemResultAttribute>(true));

        return attributes;
    }
}