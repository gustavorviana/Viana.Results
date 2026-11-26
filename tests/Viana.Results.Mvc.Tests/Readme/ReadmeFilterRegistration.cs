using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mvc.Filters;

namespace Viana.Results.Mvc.Tests.Readme;

/// <summary>Mirror of the "Global filter" registration snippet of the Mvc README.</summary>
public static class ReadmeFilterRegistration
{
    public static void Register(IServiceCollection services)
    {
        // Registers VianaResultFilter so that IResult is converted into JSON responses
        services.AddControllers().AddVianaResultFilter();

        // Or manual registration:
        services.AddControllers(options =>
        {
            options.Filters.Add<VianaResultFilter>();
        });
    }
}
