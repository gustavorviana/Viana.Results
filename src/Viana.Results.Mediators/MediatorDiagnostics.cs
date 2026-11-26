using System.Collections.Generic;

namespace Viana.Results.Mediators;

/// <summary>Names of the mediator telemetry (for <c>AddSource</c>/<c>AddMeter</c> and dashboards).</summary>
public static class MediatorDiagnostics
{
    /// <summary>Name of the <c>ActivitySource</c>.</summary>
    public const string SourceName = "Viana.Results.Mediators";

    /// <summary>Name of the <c>Meter</c>.</summary>
    public const string MeterName = "Viana.Results.Mediators";

    /// <summary>Dispatch duration histogram, in seconds (its count is the number of dispatches).</summary>
    public const string DurationInstrument = "viana.mediator.request.duration";

    private static readonly IReadOnlyList<double> DurationBuckets =
        new System.Collections.ObjectModel.ReadOnlyCollection<double>(new[] { 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 });

    /// <summary>
    /// Seconds-based bucket boundaries for <see cref="DurationInstrument"/> (the OpenTelemetry recommendation for
    /// durations in seconds). Advised automatically on .NET 9+; on older targets, set them with an OpenTelemetry View.
    /// </summary>
    public static IReadOnlyList<double> DurationBucketBoundaries => DurationBuckets;

    /// <summary>Request type tag.</summary>
    public const string RequestTypeTag = "viana.mediator.request.type";

    /// <summary>Result type tag.</summary>
    public const string ResultTypeTag = "viana.mediator.result.type";

    /// <summary>Handler implementation type tag.</summary>
    public const string HandlerTypeTag = "viana.mediator.handler.type";

    /// <summary>Success tag (no problem in the result).</summary>
    public const string SuccessTag = "viana.mediator.result.success";

    /// <summary>Result status tag.</summary>
    public const string StatusTag = "viana.mediator.result.status";

    /// <summary>Set to <c>true</c> when the dispatch was cancelled.</summary>
    public const string CancelledTag = "viana.mediator.cancelled";

    /// <summary>OpenTelemetry standard failure attribute (exception type name).</summary>
    public const string ErrorTypeTag = "error.type";
}
