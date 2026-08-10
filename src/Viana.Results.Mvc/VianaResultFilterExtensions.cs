using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Viana.Results.Mvc.Filters;

namespace Viana.Results.Mvc;

/// <summary>
/// Extension methods for registering the Viana.Results MVC integration (e.g. <see cref="VianaResultFilter"/>).
/// </summary>
public static class VianaResultFilterExtensions
{
    /// <summary>
    /// Registers <see cref="VianaResultFilter"/> in the MVC pipeline so that action results of type
    /// <see cref="IResult"/> are converted to native ASP.NET Core results and serialized by the framework
    /// (status code, RFC 9457 problem details on error, or data payload on success).
    /// </summary>
    /// <param name="builder">The MVC builder.</param>
    /// <param name="configure">Optional callback to configure <see cref="VianaResultMvcOptions"/> (e.g. add "application/problem+xml").</param>
    /// <returns>The same <see cref="IMvcBuilder"/> for chaining.</returns>
    public static IMvcBuilder AddVianaResultFilter(this IMvcBuilder builder, Action<VianaResultMvcOptions>? configure = null)
    {
        builder.Services.AddOptions<VianaResultMvcOptions>();
        if (configure != null)
            builder.Services.Configure(configure);

        builder.Services.AddScoped<VianaResultFilter>();

        builder.Services.AddTransient<IConfigureOptions<MvcOptions>>(_ =>
            new ConfigureNamedOptions<MvcOptions>(Options.DefaultName, options =>
                options.Filters.AddService<VianaResultFilter>()));

        return builder;
    }
}
