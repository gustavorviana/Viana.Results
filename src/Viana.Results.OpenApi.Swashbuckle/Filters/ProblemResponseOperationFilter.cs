using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Adds RFC 9457 problem response examples to operations decorated with <c>[ProblemResult]</c>.
/// </summary>
public class ProblemResponseOperationFilter : IOperationFilter
{
    private readonly IOptions<JsonOptions> _httpJson;

    /// <summary>Initializes the filter.</summary>
    public ProblemResponseOperationFilter(IOptions<JsonOptions> httpJson)
    {
        _httpJson = httpJson;
    }

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
        => OpenApiOperationProcessor.ApplyProblemResultAttributes(operation, context.MethodInfo, _httpJson.Value.SerializerOptions);
}
