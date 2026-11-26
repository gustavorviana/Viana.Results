using System.Text.Json;
using System.Text.Json.Nodes;

namespace Viana.Results.Tests.Envelopes;

/// <summary>Feature 014 US2: extension entries go to the root; declared members win.</summary>
public class EnvelopeExtensionsTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private static JsonObject Body(Dictionary<string, object?> extensions)
        => Assert.IsType<JsonObject>(ResultResponseBody.GetBody(
            new ExtendedTestCursorResult([new CursorItem { Id = 1 }], "abc", extensions), Web));

    [Fact]
    public void Entries_are_written_at_the_root_without_an_extensions_member()
    {
        var body = Body(new() { ["region"] = "eu", ["count"] = 3 });

        Assert.Equal("eu", body["region"]!.GetValue<string>());
        Assert.Equal(3, body["count"]!.GetValue<int>());
        Assert.False(body.ContainsKey("extensions"));
    }

    [Fact]
    public void Declared_member_wins_on_a_case_insensitive_clash()
    {
        var body = Body(new() { ["Data"] = "x", ["NEXTCURSOR"] = "y" });

        Assert.IsType<JsonArray>(body["data"]);
        Assert.Equal("abc", body["nextCursor"]!.GetValue<string>());
        // Exact keys: the Web options make JsonObject lookups case-insensitive.
        Assert.Equal(["data", "nextCursor"], body.Select(p => p.Key).OrderBy(k => k, StringComparer.Ordinal));
    }

    [Fact]
    public void Null_values_are_written_as_null()
    {
        var body = Body(new() { ["note"] = null });

        Assert.True(body.ContainsKey("note"));
        Assert.Null(body["note"]);
    }

    [Fact]
    public void Values_use_their_runtime_type()
    {
        var body = Body(new() { ["item"] = (object)new CursorItem { Id = 7 } });

        Assert.Equal(7, body["item"]!["id"]!.GetValue<int>());
    }
}
