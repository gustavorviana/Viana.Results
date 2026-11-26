using System.Text.Json.Serialization;

namespace Viana.Results.Tests.Envelopes;

/// <summary>Envelope whose Status/Problem are written only under a condition (feature 015).</summary>
public sealed class ConditionalStatusCursorResult(IReadOnlyList<CursorItem> data, int status) : IEnvelopeResult<IReadOnlyList<CursorItem>>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Status { get; } = status;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ProblemResult? Problem => null;

    public IReadOnlyList<CursorItem> Data { get; } = data;
}
