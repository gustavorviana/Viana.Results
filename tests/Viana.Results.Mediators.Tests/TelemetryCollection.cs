namespace Viana.Results.Mediators.Tests;

/// <summary>Listeners are process-wide: telemetry tests run alone (no cross-test sampling upgrades).</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class TelemetryCollection
{
    public const string Name = "telemetry";
}
