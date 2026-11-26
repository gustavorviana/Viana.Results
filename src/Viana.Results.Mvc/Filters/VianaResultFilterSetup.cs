using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Viana.Results.Mvc.Filters;

/// <summary>Adds <see cref="VianaResultFilter"/> to the MVC filters (registered once via TryAddEnumerable).</summary>
internal sealed class VianaResultFilterSetup : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options) => options.Filters.AddService<VianaResultFilter>();
}
