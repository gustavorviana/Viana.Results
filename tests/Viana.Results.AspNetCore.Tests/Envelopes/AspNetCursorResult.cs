using System.Text.Json.Serialization;

namespace Viana.Results.AspNetCore.Tests.Envelopes;

/// <summary>Cursor envelope that does not derive from <see cref="Result"/>.</summary>
public sealed class AspNetCursorResult(IReadOnlyList<int> data, string? nextCursor, IReadOnlyDictionary<string, object?>? extensions = null)
    : IEnvelopeResult<IReadOnlyList<int>>, IHasExtensions
{
    public int Status => 200;

    public ProblemResult? Problem => null;

    [JsonPropertyName("items")]
    public IReadOnlyList<int> Data { get; } = data;

    public string? NextCursor { get; } = nextCursor;

    public IReadOnlyDictionary<string, object?> Extensions { get; } = extensions ?? new Dictionary<string, object?>();
}
