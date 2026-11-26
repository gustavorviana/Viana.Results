using System.Text.Json;
using System.Text.Json.Nodes;

namespace Viana.Results.Tests.Envelopes;

/// <summary>Feature 014 US3: PagedResult is an envelope and its body does not change.</summary>
public class PagedEnvelopeTests
{
    [Fact]
    public void PagedResult_is_an_envelope_with_the_same_body()
    {
        var paged = new PagedResult<CursorItem>([new CursorItem { Id = 1 }], 2, 5);

        Assert.IsAssignableFrom<IEnvelopeResult<IReadOnlyList<CursorItem>>>(paged);

        // Its contract already hides Status/Problem, so the instance itself is the body (no extra node).
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var raw = ResultResponseBody.GetBody(paged, options);
        Assert.Same(paged, raw);
        var body = JsonSerializer.SerializeToNode(raw, raw!.GetType(), options)!.AsObject();
        Assert.Equal(["data", "pageNumber", "totalPages"], body.Select(p => p.Key).OrderBy(k => k));
        Assert.Equal(2, body["pageNumber"]!.GetValue<int>());
        Assert.Equal(5, body["totalPages"]!.GetValue<int>());
    }
}
