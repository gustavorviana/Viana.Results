namespace Viana.Results.Tests.Envelopes;

public sealed class ExtendedTestCursorResult(IReadOnlyList<CursorItem> data, string? nextCursor, IReadOnlyDictionary<string, object?> extensions)
    : IEnvelopeResult<IReadOnlyList<CursorItem>>, IHasExtensions
{
    public int Status => 200;

    public ProblemResult? Problem => null;

    public IReadOnlyList<CursorItem> Data { get; } = data;

    public string? NextCursor { get; } = nextCursor;

    public IReadOnlyDictionary<string, object?> Extensions { get; } = extensions;
}
