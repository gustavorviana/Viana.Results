namespace Viana.Results.OpenApi.Tests.Envelopes;

public sealed class ExtendedCursorResult(IReadOnlyList<int> data, IReadOnlyDictionary<string, object?> extensions)
    : IEnvelopeResult<IReadOnlyList<int>>, IHasExtensions
{
    public int Status => 200;

    public ProblemResult? Problem => null;

    public IReadOnlyList<int> Data { get; } = data;

    public IReadOnlyDictionary<string, object?> Extensions { get; } = extensions;
}
