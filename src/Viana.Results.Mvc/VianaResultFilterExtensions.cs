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
    /// <see cref="IResult"/> are converted to <see cref="VianaResultAction"/> and written as JSON
    /// (status code, problem details on error, or data payload on success).
    /// </summary>
    /// <param name="builder">The MVC builder.</param>
    /// <returns>The same <see cref="IMvcBuilder"/> for chaining.</returns>
    public static IMvcBuilder AddVianaResultFilter(this IMvcBuilder builder)
    {
        builder.Services.AddScoped<VianaResultFilter>();

        builder.Services.AddTransient<IConfigureOptions<MvcOptions>>(_ =>
            new ConfigureNamedOptions<MvcOptions>(Options.DefaultName, options =>
                options.Filters.AddService<VianaResultFilter>()));

        return builder;
    }
}
