using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Viana.Results.AspNetCore;

/// <summary>Registration of <see cref="VianaResultEndpointFilter"/> on minimal API endpoints or groups.</summary>
public static class VianaEndpointConventionExtensions
{
    /// <summary>Lets the endpoint(s) return Viana results directly (sync or async); same output as <c>ToHttpResult()</c>.</summary>
    /// <typeparam name="TBuilder">A route handler or route group builder.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    public static TBuilder AddVianaResults<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new VianaResultEndpointFilter());
}
