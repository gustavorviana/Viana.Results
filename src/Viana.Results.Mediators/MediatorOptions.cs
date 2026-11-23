using Microsoft.Extensions.DependencyInjection;

namespace Viana.Results.Mediators;

/// <summary>Options for <c>AddMediator</c>.</summary>
public sealed class MediatorOptions
{
    /// <summary>Lifetime of the scanned handlers. Default: <see cref="ServiceLifetime.Scoped"/>.</summary>
    public ServiceLifetime HandlerLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>Telemetry (traces and metrics) options.</summary>
    public MediatorTelemetryOptions Telemetry { get; } = new();
}
