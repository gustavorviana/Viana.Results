using Microsoft.OpenApi;
#if NET8_0
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

internal sealed class NoResponseBodyOperationFilter : IOperationFilter
{
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