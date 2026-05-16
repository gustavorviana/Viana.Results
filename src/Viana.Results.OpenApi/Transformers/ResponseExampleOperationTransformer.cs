using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI operation transformer that materializes <see cref="ResponseExampleAttribute"/>
/// declarations (and globally-registered examples) into <c>example</c>/<c>examples</c>
/// entries on each JSON response media type.
/// </summary>
public sealed class ResponseExampleOperationTransformer : IOpenApiOperationTransformer
{
    private readonly IOptions<JsonOptions> _httpJson;
    private readonly IServiceProvider _services;
    private readonly VianaResultOptions? _options;

    /// <summary>Initializes the transformer.</summary>
    public ResponseExampleOperationTransformer(IOptions<JsonOptions> httpJson, IServiceProvider services)
    {
        _httpJson = httpJson;
        _services = services;
        _options = services.GetService(typeof(VianaResultOptions)) as VianaResultOptions;
    }

    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var method = TransformerHelper.GetMethodInfo(context.Description);
        if (method != null)
            OpenApiOperationProcessor.ApplyResponseExamples(operation, method, _httpJson.Value.SerializerOptions, _services, _options);
        return Task.CompletedTask;
    }
}
