
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Viana.Results.AspNetCore.Filters;

namespace Viana.Results.AspNetCore
{
    public static class CustomResultFilterExtensions
    {
        public static IMvcBuilder AddCustomResultFilter(
            this IMvcBuilder builder,
            Action<ResponseFormatOptions> configure = null)
        {
            if (configure != null)
                builder.Services.Configure(configure);

            builder.Services.AddScoped<CustomResultFilter>();

            builder.Services.AddTransient<IConfigureOptions<MvcOptions>>(sp =>
            {
                return new ConfigureNamedOptions<MvcOptions>(Options.DefaultName, options =>
                {
                    options.Filters.AddService<CustomResultFilter>();
                });
            });

            return builder;
        }
    }
}
