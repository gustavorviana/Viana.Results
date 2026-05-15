using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI operation transformer that removes response bodies when the action returns <see cref="Result"/>.
/// This is useful for endpoints that intentionally return no content payload.
/// </summary>
public sealed class NoResponseBodyOperationTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation?.Responses is null || operation.Responses.Count == 0)
            return Task.CompletedTask;

        var returnType = TransformerHelper.GetMethodInfo(context.Description)?.ReturnType;
        if (returnType == null)
            return Task.CompletedTask;

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name == "Task`1")
            returnType = returnType.GetGenericArguments()[0];

        if (returnType != typeof(Result))
            return Task.CompletedTask;

        foreach (var response in operation.Responses.Values)
        {
            if (response.Content is null || response.Content.Count == 0)
                continue;

            response.Content.Clear();
            response.Description ??= "OK";
        }

        return Task.CompletedTask;
    }
}
