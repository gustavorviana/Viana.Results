using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mvc;

namespace Viana.Results.Benchmarks;

/// <summary>Viana.Results.Mvc response writing (VianaResultAction).</summary>
[MemoryDiagnoser]
public class MvcBenchmarks
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
        _services = new ServiceCollection().AddOptions().BuildServiceProvider();
        _success = Results.Ok(new WeatherLike { Date = DateTime.UnixEpoch, TemperatureC = 22, Summary = "Mild" });
        _failure = new ProblemBuilder(404).WithDetail("Not here.").BuildResult();
    }

    private ActionContext NewContext()
    {
        var http = new DefaultHttpContext { RequestServices = _services };
        http.Response.Body = new MemoryStream();
        return new ActionContext(http, new RouteData(), new ActionDescriptor());
    }

    [Benchmark]
    public Task Success() => new VianaResultAction(_success).ExecuteResultAsync(NewContext());

    [Benchmark]
    public Task Failure() => new VianaResultAction(_failure).ExecuteResultAsync(NewContext());
}
