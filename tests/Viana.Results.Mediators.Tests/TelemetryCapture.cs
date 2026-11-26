using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Viana.Results.Mediators.Tests;

/// <summary>Collects mediator spans/measurements whose request type name matches (tests run in parallel).</summary>
internal sealed class TelemetryCapture : IDisposable
{
    private readonly ActivityListener? _activities;
    private readonly MeterListener? _meters;
    private readonly string _requestName;

    public ConcurrentQueue<Activity> Spans { get; } = new();
    public ConcurrentQueue<(double Value, Dictionary<string, object?> Tags)> Measurements { get; } = new();

    public TelemetryCapture(string requestName, bool traces = true, bool metrics = true,
        ActivitySamplingResult sampling = ActivitySamplingResult.AllDataAndRecorded)
    {
        _requestName = requestName;
        if (traces)
        {
            _activities = new ActivityListener
            {
                ShouldListenTo = s => s.Name == MediatorDiagnostics.SourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => sampling,
                ActivityStopped = a => { if (a.DisplayName == "Mediator.Send " + _requestName) Spans.Enqueue(a); }
            };
            ActivitySource.AddActivityListener(_activities);
        }

        if (metrics)
        {
            _meters = new MeterListener
            {
                InstrumentPublished = (i, l) =>
                {
                    if (i.Meter.Name == MediatorDiagnostics.MeterName && i.Name == MediatorDiagnostics.DurationInstrument)
                        l.EnableMeasurementEvents(i);
                }
            };
            _meters.SetMeasurementEventCallback<double>((_, value, tags, _) =>
            {
                var dict = new Dictionary<string, object?>();
                foreach (var t in tags)
                    dict[t.Key] = t.Value;
                if (dict.TryGetValue(MediatorDiagnostics.RequestTypeTag, out var r) && (string?)r == _requestName)
                    Measurements.Enqueue((value, dict));
            });
            _meters.Start();
        }
    }

    public Activity Single() => Assert.Single(Spans);

    public void Dispose()
    {
        _activities?.Dispose();
        _meters?.Dispose();
    }
}
