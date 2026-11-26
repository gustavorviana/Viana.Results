using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Viana.Results.AspNetCore;

/// <summary>
/// Endpoint filter that lets minimal API handlers return Viana results directly: a returned
/// <see cref="Viana.Results.IResult"/> is written as a <see cref="VianaHttpResult"/>; other values are untouched.
/// </summary>
public sealed class VianaResultEndpointFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var value = await next(context);
        return value is Viana.Results.IResult result ? new VianaHttpResult(result) : value;
    }
}
