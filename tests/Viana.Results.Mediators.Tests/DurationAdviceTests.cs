using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Viana.Results.Mediators.Tests;

/// <summary>Feature 012 US3: the duration histogram (seconds) advises seconds-based buckets on .NET 9+.</summary>
[Collection(TelemetryCollection.Name)]
public class DurationAdviceTests
{
    private static readonly double[] Expected = [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10];

    [Fact]
    public void Boundaries_are_the_OpenTelemetry_seconds_buckets()
        => Assert.Equal(Expected, MediatorDiagnostics.DurationBucketBoundaries);

#if NET9_0_OR_GREATER
    public sealed class Ping : IRequest<Result> { }

    public sealed class PingHandler : IHandler<Ping, Result>
    {
        public Task<Result> Handle(Ping request, CancellationToken cancellationToken = default) => Task.FromResult(Results.Ok());
    }

    [Fact]
    public async Task Histogram_carries_the_bucket_advice()
    {
        Instrument? duration = null;
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == MediatorDiagnostics.MeterName && instrument.Name == MediatorDiagnostics.DurationInstrument)
                {
                    duration = instrument;
                    l.EnableMeasurementEvents(instrument);
                }
            }
        };
        listener.Start();

        var services = new ServiceCollection();
        services.AddScoped<IHandler<Ping, Result>, PingHandler>();
        using var provider = services.BuildServiceProvider();
        await new Mediator(provider).SendAsync(new Ping());

        var histogram = Assert.IsType<Histogram<double>>(duration);
        Assert.Equal(Expected, histogram.Advice!.HistogramBucketBoundaries);
    }
#endif
}
