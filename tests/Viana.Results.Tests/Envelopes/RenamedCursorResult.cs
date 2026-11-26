using System.Text.Json.Serialization;

namespace Viana.Results.Tests.Envelopes;

public sealed class RenamedCursorResult(IReadOnlyList<CursorItem> data) : IEnvelopeResult<IReadOnlyList<CursorItem>>
{
    public int Status => 200;

    public ProblemResult? Problem => null;

    [JsonPropertyName("items")]
    public IReadOnlyList<CursorItem> Data { get; } = data;
}
