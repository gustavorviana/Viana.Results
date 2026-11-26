using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Viana.Results.OpenApi;

/// <summary>
/// DI extensions for registering Viana.Results OpenAPI-wide options
/// (currently global response examples).
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="VianaResultOptions"/> populated by <paramref name="configure"/>.
    /// Filters/transformers resolve this via DI to apply global response examples.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration callback that builds the options instance.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddVianaResultExamples(this IServiceCollection services, Action<VianaResultOptions> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        // Composes across calls (options pattern); VianaResultOptions stays resolvable directly.
        services.Configure(configure);
        services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<VianaResultOptions>>().Value);
        return services;
    }
}
