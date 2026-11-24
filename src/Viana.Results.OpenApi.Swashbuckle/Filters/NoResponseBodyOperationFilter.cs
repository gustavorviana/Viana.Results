using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Swagger operation filter that documents the success of endpoints returning a Viana result: a bare
/// <see cref="Result"/> is <c>204 No Content</c>, hand-declared successes replace the inferred one, and
/// <see cref="WithoutDefaultResultAttribute"/> keeps only the hand-registered successes.
/// </summary>
public class NoResponseBodyOperationFilter : IOperationFilter
{
    private readonly IServiceProvider? _services;

    /// <summary>Initializes the filter.</summary>
    public NoResponseBodyOperationFilter(IServiceProvider? services = null) => _services = services;

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
        => OperationProcessor.ApplyDefaultResult(
            operation,
            context.MethodInfo,
            context.ApiDescription.ActionDescriptor?.EndpointMetadata?.ToList(),
            _services,
            $"{context.ApiDescription.HttpMethod} /{context.ApiDescription.RelativePath}");
}
