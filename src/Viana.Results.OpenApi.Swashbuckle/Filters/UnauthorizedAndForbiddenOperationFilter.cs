using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Automatically adds 401 and 403 RFC 9457 responses for endpoints protected by <c>[Authorize]</c>.
/// </summary>
public class UnauthorizedAndForbiddenOperationFilter : IOperationFilter
{
    private readonly IOptions<JsonOptions> _httpJson;

    /// <summary>Initializes the filter.</summary>
    public UnauthorizedAndForbiddenOperationFilter(IOptions<JsonOptions> httpJson)
    {
        _httpJson = httpJson;
    }

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
        => OpenApiOperationProcessor.ApplyAuthorizeResponses(operation, context.MethodInfo, _httpJson.Value.SerializerOptions);
}
