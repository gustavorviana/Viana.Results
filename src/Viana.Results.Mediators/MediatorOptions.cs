namespace Viana.Results.Mediators;

/// <summary>Options for <c>AddMediator</c>.</summary>
/// <remarks>
/// Scanned handlers are registered as scoped. To give a handler another lifetime, register it yourself
/// before calling <c>AddMediator</c>; the scan keeps that registration.
/// </remarks>
public sealed class MediatorOptions
{
    /// <summary>Telemetry (traces and metrics) options.</summary>
    public MediatorTelemetryOptions Telemetry { get; } = new();
}
