namespace Viana.Results.Mvc.Tests.Envelopes;

public sealed class MvcCursorResult(IReadOnlyList<int> data, string? nextCursor, int status = 200, ProblemResult? problem = null)
    : IEnvelopeResult<IReadOnlyList<int>>
{
    public int Status { get; } = problem?.Status ?? status;

    public ProblemResult? Problem { get; } = problem;

    public IReadOnlyList<int> Data { get; } = data;

    public string? NextCursor { get; } = nextCursor;
}
