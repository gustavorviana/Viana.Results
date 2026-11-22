using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Viana.Results.AspNetCore.Filters;

namespace Viana.Results.AspNetCore;

/// <summary>Registration of the opt-in global <see cref="VianaResultFilter"/>.</summary>
public static class VianaResultFilterExtensions
{
    /// <summary>
    /// Lets actions return Viana results directly (sync or async); responses are identical to
    /// <see cref="ResultActionResultExtensions.ToActionResult(IResult)"/>. Calling it more than once registers the filter once.
    /// </summary>
    public static IMvcBuilder AddVianaResultFilter(this IMvcBuilder builder)
    {
        builder.Services.TryAddScoped<VianaResultFilter>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, VianaResultFilterSetup>());
        return builder;
    }
}
