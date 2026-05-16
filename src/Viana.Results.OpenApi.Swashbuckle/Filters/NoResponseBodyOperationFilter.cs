using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Swagger operation filter that removes response bodies when the action returns <see cref="Result"/>.
/// </summary>
public class NoResponseBodyOperationFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
        => OpenApiOperationProcessor.ClearBodyForBareResult(operation, context.MethodInfo);
}
