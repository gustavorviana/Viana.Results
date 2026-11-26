using System.Diagnostics;
using System.Diagnostics.Metrics;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mediators;

namespace Viana.Results.Benchmarks;

/// <summary>Mediator dispatch cost (feature 009). "NewMediator" mirrors one mediator per request scope.</summary>
[MemoryDiagnoser]
public class MediatorBenchmarks
{
    public sealed class Ping : IRequest<Result<int>> { public int Value { get; init; } }

    public sealed class PingHandler : IHandler<Ping, Result<int>>
    {
        public Task<Result<int>> Handle(Ping request, CancellationToken cancellationToken = default)
            => Task.FromResult(Results.Ok(request.Value));
    }

    private const string Source = "Viana.Results.Mediators";
    private readonly Ping _ping = new() { Value = 1 };
    private IServiceProvider _provider = null!;
    private IMediator _mediator = null!;
    private PingHandler _handler = null!;
    private ActivityListener? _activities;
    private MeterListener? _meters;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddScoped<IHandler<Ping, Result<int>>, PingHandler>();
        _provider = services.BuildServiceProvider().CreateScope().ServiceProvider;
        _mediator = new Mediator(_provider);
        _handler = new PingHandler();
    }

    [GlobalSetup(Targets = new[] { nameof(Send_WithListener) })]
    public void SetupWithListener()
    {
        Setup();
        _activities = new ActivityListener
        {
            ShouldListenTo = s => s.Name == Source,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(_activities);
        _meters = new MeterListener { InstrumentPublished = (i, l) => { if (i.Meter.Name == Source) l.EnableMeasurementEvents(i); } };
        _meters.SetMeasurementEventCallback<double>((_, _, _, _) => { });
        _meters.Start();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _activities?.Dispose();
        _meters?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public Task<Result<int>> DirectHandler() => _handler.Handle(_ping);

    [Benchmark]
    public Task<Result<int>> Send() => _mediator.SendAsync(_ping);

    [Benchmark]
    public Task<Result<int>> Send_NewMediator() => new Mediator(_provider).SendAsync(_ping);

    [Benchmark]
    public Task<Result<int>> Send_WithListener() => _mediator.SendAsync(_ping);

    private IMediator _off = null!;

    [GlobalSetup(Targets = new[] { nameof(Send_TelemetryOff) })]
    public void SetupTelemetryOff()
    {
        SetupWithListener(); // listeners active, but both kinds disabled: must not be consulted
        var options = new MediatorOptions();
        options.Telemetry.Tracing = false;
        options.Telemetry.Metrics = false;
        _off = new Mediator(_provider, options);
    }

    [Benchmark]
    public Task<Result<int>> Send_TelemetryOff() => _off.SendAsync(_ping);
}
