namespace Viana.Results.Mediators;

/// <summary>
/// Telemetry options. Traces and metrics are emitted on .NET 6+ and cost nothing without a listener;
/// a disabled kind is never consulted, even with active listeners.
/// </summary>
public sealed class MediatorTelemetryOptions
{
    /// <summary>Creates one span per dispatch. Default: <c>true</c>.</summary>
    public bool Tracing { get; set; } = true;

    /// <summary>Records the dispatch duration histogram. Default: <c>true</c>.</summary>
    public bool Metrics { get; set; } = true;

    /// <summary>How much of an exception is recorded on the span. Default: <see cref="ExceptionDetailLevel.Full"/>.</summary>
    public ExceptionDetailLevel ExceptionDetails { get; set; } = ExceptionDetailLevel.Full;
}
