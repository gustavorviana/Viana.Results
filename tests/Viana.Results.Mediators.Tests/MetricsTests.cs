namespace Viana.Results.Mediators.Tests;

/// <summary>Feature 009 US2: dispatch duration histogram.</summary>
[Collection(TelemetryCollection.Name)]
public class MetricsTests
{
    public sealed class MOk : IRequest<Result> { }
    public sealed class MOkHandler : IHandler<MOk, Result> { public Task<Result> Handle(MOk r, CancellationToken ct = default) => Task.FromResult(Results.Ok()); }

    public sealed class MFail : IRequest<Result> { }
    public sealed class MFailHandler : IHandler<MFail, Result> { public Task<Result> Handle(MFail r, CancellationToken ct = default) => throw new InvalidTimeZoneException(); }

    private static IMediator Mediator() => TelemetryTests.Mediator(host: typeof(MetricsTests));

    [Fact]
    public async Task OneMeasurementPerSend_WithTags()
    {
        using var capture = new TelemetryCapture(nameof(MOk), traces: false);
        var mediator = Mediator();

        await mediator.SendAsync(new MOk());
        await mediator.SendAsync(new MOk());

        Assert.Equal(2, capture.Measurements.Count);
        var (value, tags) = capture.Measurements.First();
        Assert.True(value >= 0);
        Assert.Equal(200, tags[MediatorDiagnostics.StatusTag]);
        Assert.Equal(true, tags[MediatorDiagnostics.SuccessTag]);
        Assert.False(tags.ContainsKey(MediatorDiagnostics.ErrorTypeTag));
    }

    [Fact]
    public async Task Failure_AddsErrorType()
    {
        using var capture = new TelemetryCapture(nameof(MFail), traces: false);

        await TelemetryTests.Swallow(() => Mediator().SendAsync(new MFail()));

        var (_, tags) = Assert.Single(capture.Measurements);
        Assert.Equal(typeof(InvalidTimeZoneException).FullName, tags[MediatorDiagnostics.ErrorTypeTag]);
        Assert.Equal(false, tags[MediatorDiagnostics.SuccessTag]);
    }
}
