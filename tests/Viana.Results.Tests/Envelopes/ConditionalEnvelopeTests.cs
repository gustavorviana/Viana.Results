using System.Text.Json;

namespace Viana.Results.Tests.Envelopes;

/// <summary>Feature 015 US1: the shaping decision comes from the contract, not from the first instance.</summary>
public class ConditionalEnvelopeTests
{
    private static readonly CursorItem[] Items = [new() { Id = 1 }];

    private static string[] Keys(object? body, JsonSerializerOptions options)
        => JsonSerializer.SerializeToNode(body, body!.GetType(), options)!.AsObject().Select(p => p.Key).ToArray();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Conditional_status_and_problem_never_reach_the_body(bool defaultFirst)
    {
        // Fresh options per case, so the per-contract cache starts empty and the order matters.
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var zero = new ConditionalStatusCursorResult(Items, 0);
        var ok = new ConditionalStatusCursorResult(Items, 200);
        var order = defaultFirst ? new[] { zero, ok } : new[] { ok, zero };

        foreach (var result in order)
        {
            var keys = Keys(ResultResponseBody.GetBody(result, options), options);
            Assert.DoesNotContain("status", keys);
            Assert.DoesNotContain("problem", keys);
            Assert.Contains("data", keys);
        }
    }

    [Fact]
    public void PagedResult_is_still_the_instance()
    {
        var paged = new PagedResult<CursorItem>(Items, 1, 1);

        Assert.Same(paged, ResultResponseBody.GetBody(paged, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
