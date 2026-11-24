using Microsoft.Extensions.DependencyInjection;

namespace Viana.Results.Mediators.Tests;

/// <summary>Feature 009 US5: telemetry switches.</summary>
[Collection(TelemetryCollection.Name)]
public class OptionsTests
{
    public sealed class OTraceOff : IRequest<Result> { }
    public sealed class OTraceOffHandler : IHandler<OTraceOff, Result> { public Task<Result> Handle(OTraceOff r, CancellationToken ct = default) => Task.FromResult(Results.Ok()); }
    public sealed class OMetricsOff : IRequest<Result> { }
    public sealed class OMetricsOffHandler : IHandler<OMetricsOff, Result> { public Task<Result> Handle(OMetricsOff r, CancellationToken ct = default) => Task.FromResult(Results.Ok()); }
    public sealed class OBothOff : IRequest<Result> { }
    public sealed class OBothOffHandler : IHandler<OBothOff, Result> { public Task<Result> Handle(OBothOff r, CancellationToken ct = default) => Task.FromResult(Results.Ok()); }

    private static IMediator Mediator(Action<MediatorOptions> configure) => TelemetryTests.Mediator(configure, typeof(OptionsTests));

    [Fact]
    public void Defaults()
    {
        var options = new MediatorOptions();

        Assert.True(options.Telemetry.Tracing);
        Assert.True(options.Telemetry.Metrics);
        Assert.Equal(ExceptionDetailLevel.Full, options.Telemetry.ExceptionDetails);
    }

    [Fact]
    public async Task TracingOff_NoSpan_MetricsStillRecorded()
    {
        using var capture = new TelemetryCapture(nameof(OTraceOff));

        await Mediator(o => o.Telemetry.Tracing = false).SendAsync(new OTraceOff());

        Assert.Empty(capture.Spans);
        Assert.Single(capture.Measurements);
    }

    [Fact]
    public async Task MetricsOff_NoMeasurement_SpanStillCreated()
    {
        using var capture = new TelemetryCapture(nameof(OMetricsOff));

        await Mediator(o => o.Telemetry.Metrics = false).SendAsync(new OMetricsOff());

        Assert.Single(capture.Spans);
        Assert.Empty(capture.Measurements);
    }

    [Fact]
    public async Task BothOff_Nothing()
    {
        using var capture = new TelemetryCapture(nameof(OBothOff));

        await Mediator(o => { o.Telemetry.Tracing = false; o.Telemetry.Metrics = false; }).SendAsync(new OBothOff());

        Assert.Empty(capture.Spans);
        Assert.Empty(capture.Measurements);
    }
}
