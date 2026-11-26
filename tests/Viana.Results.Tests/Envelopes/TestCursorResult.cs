using System.Text.Json.Serialization;

namespace Viana.Results.Tests.Envelopes;

/// <summary>An envelope that does not derive from <see cref="Result"/>: Status/Problem are plain properties.</summary>
public sealed class TestCursorResult(IReadOnlyList<CursorItem>? data, string? nextCursor, int status = 200, ProblemResult? problem = null)
    : IEnvelopeResult<IReadOnlyList<CursorItem>>
{
    public int Status { get; } = problem?.Status ?? status;

    public ProblemResult? Problem { get; } = problem;

    public IReadOnlyList<CursorItem>? Data { get; } = data;

    public string? NextCursor { get; } = nextCursor;

    [JsonIgnore]
    public string Secret => "hidden";
}
