namespace Viana.Results.Examples.Tests;

/// <summary>Activity and meter listeners are process-wide: telemetry tests run one at a time.</summary>
[CollectionDefinition(nameof(TelemetryCollection), DisableParallelization = true)]
public sealed class TelemetryCollection;
