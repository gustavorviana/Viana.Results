using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Viana.Results.OpenApi;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Swagger operation filter that materializes <see cref="ResponseExampleAttribute"/> declarations
/// (and globally-registered examples in <see cref="VianaResultOptions"/>) into <c>example</c>
/// or <c>examples</c> entries on the relevant JSON response media types.
/// </summary>
public sealed class ResponseExampleOperationFilter : IOperationFilter
{
    private readonly IOptions<JsonOptions> _httpJson;
    private readonly IServiceProvider _services;
    private readonly VianaResultOptions? _resultOptions;

    /// <summary>Initializes the filter.</summary>
    public ResponseExampleOperationFilter(IOptions<JsonOptions> httpJson, IServiceProvider services)
    {
        _httpJson = httpJson;
        _services = services;
        _resultOptions = services.GetService(typeof(VianaResultOptions)) as VianaResultOptions;
    }

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null || operation.Responses.Count == 0)
            return;

        var globals = _resultOptions?.GlobalExamples ?? Array.Empty<GlobalExampleRegistration>();
        var byStatus = ResponseExampleResolver.Collect(context.MethodInfo, globals);
        if (byStatus.Count == 0)
            return;

        foreach (var pair in byStatus)
        {
            var statusKey = pair.Key.ToString();
            if (!operation.Responses.TryGetValue(statusKey, out var response) || response.Content == null)
                continue;

            ApplyExamples(response, pair.Value);
        }
    }

    private void ApplyExamples(IOpenApiResponse response, List<ResolvedExample> examples)
    {
        var serializerOptions = _httpJson.Value.SerializerOptions;
        var built = new List<(string Key, ResolvedExample Meta, System.Text.Json.Nodes.JsonNode? Node)>();

        foreach (var ex in examples)
        {
            var provider = ResponseExampleResolver.CreateProvider(ex.ProviderType, _services);
            if (provider == null)
                continue;

            var value = ResponseExampleResolver.InvokeGetExample(provider);
            var node = ResponseExampleResolver.Serialize(value, serializerOptions);
            var key = ex.Name ?? ex.ProviderType.Name;
            built.Add((key, ex, node));
        }

        if (built.Count == 0)
            return;

        var useExamplesMap = built.Count > 1 || built.Any(b => b.Meta.Name != null);

        foreach (var media in response.Content!.Values)
        {
            if (useExamplesMap)
            {
                media.Example = null;
                media.Examples ??= new Dictionary<string, IOpenApiExample>();
                foreach (var (key, meta, node) in built)
                {
                    media.Examples[key] = new OpenApiExample
                    {
                        Value = node,
                        Summary = meta.Summary,
                        Description = meta.Description
                    };
                }
            }
            else
            {
                media.Example = built[0].Node;
            }
        }
    }
}
