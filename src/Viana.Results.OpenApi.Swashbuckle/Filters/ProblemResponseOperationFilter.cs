using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Adds RFC 9457 problem response examples to operations decorated with <c>[ProblemResult]</c>.
/// </summary>
public class ProblemResponseOperationFilter : IOperationFilter
{
    private readonly IOptions<JsonOptions> _httpJson;
    private readonly IServiceProvider? _services;

    /// <summary>Initializes the filter.</summary>
    public ProblemResponseOperationFilter(IOptions<JsonOptions> httpJson, IServiceProvider? services = null)
    {
        _httpJson = httpJson;
        _services = services;
    }

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
        => OperationProcessor.ApplyProblemResultAttributes(
            operation,
            context.MethodInfo,
            _httpJson.Value.SerializerOptions,
            EndpointScope.From(context.ApiDescription.ActionDescriptor, _services));
}
