using System.Text.Json.Serialization;

namespace Viana.Results.OpenApi.Tests.Envelopes;

public sealed class DocCursorResult(IReadOnlyList<int> data, string? nextCursor) : IEnvelopeResult<IReadOnlyList<int>>
{
    public int Status => 200;

    public ProblemResult? Problem => null;

    [JsonPropertyName("items")]
    public IReadOnlyList<int> Data { get; } = data;

    public string? NextCursor { get; } = nextCursor;
}
