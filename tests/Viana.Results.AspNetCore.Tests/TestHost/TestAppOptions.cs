using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Viana.Results.AspNetCore.Tests.TestHost;

/// <summary>Options for an in-memory test application.</summary>
public sealed class TestAppOptions
{
    /// <summary>Registers <c>AddProblemDetails()</c> (the native problem details service).</summary>
    public bool AddProblemDetails { get; set; } = true;

    /// <summary>Optional <c>CustomizeProblemDetails</c> callback (only with <see cref="AddProblemDetails"/>).</summary>
    public Action<ProblemDetailsContext>? Customize { get; set; }

    /// <summary>Extra MVC configuration (formatters, Newtonsoft, filters...).</summary>
    public Action<IMvcBuilder>? Mvc { get; set; }

    /// <summary>Extra service registrations.</summary>
    public Action<IServiceCollection>? Services { get; set; }

    /// <summary>Extra endpoints (minimal APIs).</summary>
    public Action<WebApplication>? Endpoints { get; set; }
}
