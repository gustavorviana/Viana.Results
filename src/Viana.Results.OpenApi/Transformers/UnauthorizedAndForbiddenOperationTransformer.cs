using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.OpenApi.Schemas;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI operation transformer that automatically adds standard RFC 9457
/// responses for <c>401 Unauthorized</c> and <c>403 Forbidden</c> when an
/// endpoint is protected by <see cref="AuthorizeAttribute"/>.
/// </summary>
public sealed class UnauthorizedAndForbiddenOperationTransformer : IOpenApiOperationTransformer
{
    private readonly IOptions<JsonOptions> _httpJson;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedAndForbiddenOperationTransformer"/> class.
    /// </summary>
    /// <param name="httpJson">Provides access to the configured JSON serializer options.</param>
    public UnauthorizedAndForbiddenOperationTransformer(IOptions<JsonOptions> httpJson)
    {
        _httpJson = httpJson;
    }

    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var methodInfo = TransformerHelper.GetMethodInfo(context.Description);
        if (methodInfo == null)
            return Task.CompletedTask;

        var hasAuthorize = methodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || methodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;

        if (!hasAuthorize)
            return Task.CompletedTask;

        new ProblemResultSchema(_httpJson.Value.SerializerOptions, 401).ApplyTo(operation.Responses);
        new ProblemResultSchema(_httpJson.Value.SerializerOptions, 403).ApplyTo(operation.Responses);

        return Task.CompletedTask;
    }
}
