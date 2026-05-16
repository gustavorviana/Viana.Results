using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI operation transformer that automatically adds standard RFC 9457
/// responses for 401 Unauthorized and 403 Forbidden when an endpoint is protected
/// by <c>[Authorize]</c>.
/// </summary>
public sealed class UnauthorizedAndForbiddenOperationTransformer : IOpenApiOperationTransformer
{
    private readonly IOptions<JsonOptions> _httpJson;

    /// <summary>Initializes the transformer.</summary>
    public UnauthorizedAndForbiddenOperationTransformer(IOptions<JsonOptions> httpJson)
    {
        _httpJson = httpJson;
    }

    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var method = TransformerHelper.GetMethodInfo(context.Description);
        if (method != null)
            OpenApiOperationProcessor.ApplyAuthorizeResponses(operation, method, _httpJson.Value.SerializerOptions);
        return Task.CompletedTask;
    }
}
