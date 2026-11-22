using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests;

/// <summary>
/// Research R1 gate: verifies how the native <see cref="IProblemDetailsService"/> treats a ProblemDetails
/// that already carries values, inside an MVC action.
/// </summary>
public class ProblemPipelineSpikeTests
{
    [Fact]
    public async Task ProblemDetailsService_FillsOnlyMissing_AndCustomizeRunsLastOnce()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions
        {
            Customize = ctx => ctx.ProblemDetails.Extensions["customized"] = ctx.ProblemDetails.Title
        });

        var response = await app.Client.GetAsync("/spike");
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        // (a) native behavior differs per framework: net8 keeps a preset traceId, net10 overwrites it.
        // The ASP.NET Core package therefore always lets the native traceId win (FR-003a).
#if NET10_0_OR_GREATER
        Assert.NotEqual("viana-trace", root.GetProperty("traceId").GetString());
#else
        Assert.Equal("viana-trace", root.GetProperty("traceId").GetString());
#endif
        // (b) missing type gets the native RFC 9110 link
        Assert.StartsWith("https://tools.ietf.org/html/rfc9110", root.GetProperty("type").GetString());
        // (c) customization runs after the preset values (sees the Viana title)
        Assert.Equal("Viana title", root.GetProperty("customized").GetString());
        // (d) customization invoked exactly once
        Assert.Equal(1, app.CustomizeCalls);
    }
}

[ApiController]
[Route("spike")]
public sealed class SpikeController : ControllerBase
{
    [HttpGet]
    public async Task Get([FromServices] IProblemDetailsService problems)
    {
        HttpContext.Response.StatusCode = 404;
        HttpContext.Response.Headers["x-spike"] = "hit";
        await problems.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = HttpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = 404,
                Title = "Viana title",
                Type = null,
                Extensions = { ["traceId"] = "viana-trace" }
            }
        });
    }
}
