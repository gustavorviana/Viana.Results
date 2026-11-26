namespace Viana.Results.AotSmoke;

/// <summary>Envelope that does not derive from Result, so its body is shaped (Status/Problem removed).</summary>
public sealed class SmokeCursorResult(IReadOnlyList<SmokeItem> data, string? nextCursor) : IEnvelopeResult<IReadOnlyList<SmokeItem>>
{
    public int Status => 200;

    public ProblemResult? Problem => null;

    public IReadOnlyList<SmokeItem> Data { get; } = data;

    public string? NextCursor { get; } = nextCursor;
}
