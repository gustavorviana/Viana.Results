using Microsoft.OpenApi;
#if NET8_0
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Swagger operation filter that removes response bodies when the action returns <see cref="Result"/>.
/// This is useful for endpoints that intentionally return no content payload.
/// </summary>
public class NoResponseBodyOperationFilter : IOperationFilter
{
    /// <summary>
    /// Applies the filter to the specified OpenAPI operation.
    /// If the action return type is <see cref="Result"/>, all response content is cleared.
    /// </summary>
    /// <param name="operation">The OpenAPI operation being processed.</param>
    /// <param name="context">The operation filter context.</param>

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation?.Responses is null || operation.Responses.Count == 0)
            return;

        var needClean = context.MethodInfo.ReturnType == typeof(Result);

        foreach (var response in operation.Responses.Values)
        {
            if (!needClean || response.Content is null || response.Content.Count == 0)
                continue;

            response.Content.Clear();
            response.Description ??= "OK";
        }
    }
}