using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using Viana.Results.OpenApi;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Materializes <see cref="ResponseExampleAttribute"/> declarations (and globally-registered
/// examples in <see cref="VianaResultOptions"/>) into <c>example</c>/<c>examples</c> entries
/// on each JSON response media type.
/// </summary>
public sealed class ResponseExampleOperationFilter : IOperationFilter
{
    private readonly IOptions<JsonOptions> _httpJson;
    private readonly IServiceProvider _services;
    private readonly VianaResultOptions? _options;

    /// <summary>Initializes the filter.</summary>
    public ResponseExampleOperationFilter(IOptions<JsonOptions> httpJson, IServiceProvider services)
    {
        _httpJson = httpJson;
        _services = services;
        _options = services.GetService(typeof(VianaResultOptions)) as VianaResultOptions;
    }

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var json = _httpJson.Value.SerializerOptions;
        var scope = EndpointScope.From(context.ApiDescription.ActionDescriptor, _services);
        OperationProcessor.ApplyResponseExamples(operation, context.MethodInfo, json, _services, _options, scope);

        // Minimal API declarations: Swashbuckle's schema generator is synchronous, so a synchronous filter
        // covers both GetSwagger and GetSwaggerAsync without waiting on asynchronous code.
        OperationProcessor.ApplyEndpointDeclarations(
            operation,
            context.ApiDescription.ActionDescriptor?.EndpointMetadata,
            json,
            _services,
            type => context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository),
            $"{context.ApiDescription.HttpMethod} /{context.ApiDescription.RelativePath}",
            context.MethodInfo,
            scope);
    }
}
