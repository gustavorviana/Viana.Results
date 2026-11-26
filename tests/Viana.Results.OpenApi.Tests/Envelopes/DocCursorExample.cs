namespace Viana.Results.OpenApi.Tests.Envelopes;

public sealed class DocCursorExample : IExampleProvider<DocCursorResult>
{
    public DocCursorResult GetExample() => new([1, 2], "abc");
}
