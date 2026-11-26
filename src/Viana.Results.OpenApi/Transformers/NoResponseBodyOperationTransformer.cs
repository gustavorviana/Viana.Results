using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI operation transformer that documents the success of endpoints returning a Viana result: a bare
/// <see cref="Result"/> is <c>204 No Content</c>, hand-declared successes replace the inferred one, and
/// <see cref="WithoutDefaultResultAttribute"/> keeps only the hand-registered successes.
/// </summary>
public sealed class NoResponseBodyOperationTransformer : IOpenApiOperationTransformer
{
    private readonly IServiceProvider? _services;

    /// <summary>Initializes the transformer.</summary>
    public NoResponseBodyOperationTransformer(IServiceProvider? services = null) => _services = services;

    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var method = TransformerHelper.GetMethodInfo(context.Description);
        if (method != null)
        {
            OperationProcessor.ApplyDefaultResult(
                operation,
                method,
                context.Description.ActionDescriptor?.EndpointMetadata?.ToList(),
                _services ?? context.ApplicationServices,
                $"{context.Description.HttpMethod} /{context.Description.RelativePath}",
                EndpointScope.From(context.Description.ActionDescriptor, _services ?? context.ApplicationServices));
        }
        return Task.CompletedTask;
    }
}
