using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
#if NET8_0
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Viana.Results.OpenApi.Swashbuckle.Schemas;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Swagger operation filter that automatically adds standard RFC 9457
/// responses for <c>401 Unauthorized</c> and <c>403 Forbidden</c> when an
/// endpoint is protected by <see cref="AuthorizeAttribute"/>.
/// </summary>
public class UnauthorizedAndForbiddenOperationFilter : IOperationFilter
{
    private readonly IOptions<JsonOptions> _httpJson;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedAndForbiddenOperationFilter"/> class.
    /// </summary>
    /// <param name="httpJson">Provides access to the configured JSON serializer options.</param>
    public UnauthorizedAndForbiddenOperationFilter(IOptions<JsonOptions> httpJson)
    {
        _httpJson = httpJson;
    }

    /// <summary>
    /// Applies the filter to the specified OpenAPI operation.
    /// If the action or its declaring type contains <see cref="AuthorizeAttribute"/>,
    /// standard problem responses for 401 and 403 are added.
    /// </summary>
    /// <param name="operation">The OpenAPI operation being processed.</param>
    /// <param name="context">The operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;

        if (!hasAuthorize)
            return;

        new ProblemResultSchema(_httpJson.Value.SerializerOptions, 401).ApplyTo(operation.Responses);
        new ProblemResultSchema(_httpJson.Value.SerializerOptions, 403).ApplyTo(operation.Responses);
    }
}