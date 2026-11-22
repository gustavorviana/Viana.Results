using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.AspNetCore;
using Viana.Results.Mvc;

namespace Viana.Results.Benchmarks;

/// <summary>Viana.Results.AspNetCore (native pipeline) vs Viana.Results.Mvc (hand-written JSON).</summary>
[MemoryDiagnoser]
public class AspNetCoreBenchmarks
{
    public sealed class WeatherLike
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }
    }

    private IServiceProvider _services = null!;
    private Result<WeatherLike> _success = null!;
    private Result _failure = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        services.AddControllers();
        _services = services.BuildServiceProvider();
        _success = Results.Ok(new WeatherLike { Date = DateTime.UnixEpoch, TemperatureC = 22, Summary = "Mild" });
        _failure = new ProblemBuilder(404).WithDetail("Not here.").BuildResult();
    }

    private ActionContext NewContext()
    {
        var http = new DefaultHttpContext { RequestServices = _services };
        http.Response.Body = new MemoryStream();
        return new ActionContext(http, new RouteData(), new ActionDescriptor());
    }

    [Benchmark(Baseline = true)]
    public Task MvcSuccess() => new VianaResultAction(_success).ExecuteResultAsync(NewContext());

    [Benchmark]
    public Task AspNetCoreSuccess() => new VianaActionResult(_success).ExecuteResultAsync(NewContext());

    [Benchmark]
    public Task MvcFailure() => new VianaResultAction(_failure).ExecuteResultAsync(NewContext());

    [Benchmark]
    public Task AspNetCoreFailure() => new VianaActionResult(_failure).ExecuteResultAsync(NewContext());

    [Benchmark]
    public Task MinimalSuccess() => new VianaHttpResult(_success).ExecuteAsync(NewContext().HttpContext);

    [Benchmark]
    public Task MinimalFailure() => new VianaHttpResult(_failure).ExecuteAsync(NewContext().HttpContext);
}
