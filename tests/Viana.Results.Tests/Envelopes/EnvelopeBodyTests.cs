using System.Text.Json;
using System.Text.Json.Nodes;

namespace Viana.Results.Tests.Envelopes;

/// <summary>Feature 014: the body of an envelope is the whole object minus Status/Problem.</summary>
public class EnvelopeBodyTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private static readonly CursorItem[] Items = [new() { Id = 1 }, new() { Id = 2 }];

    private static JsonObject Body(IResult result) => Assert.IsType<JsonObject>(ResultResponseBody.GetBody(result, Web));

    [Fact]
    public void Envelope_types_are_detected_and_never_unwrapped()
    {
        Assert.True(ResultReflections.IsEnvelopeType(typeof(TestCursorResult)));
        Assert.True(ResultReflections.IsEnvelopeType(typeof(PagedResult<CursorItem>)));
        Assert.False(ResultReflections.IsEnvelopeType(typeof(Result<CursorItem>)));
        Assert.False(ResultReflections.IsEnvelopeType(typeof(ListResult<CursorItem>)));
        Assert.False(ResultReflections.IsUnwrappableType(typeof(TestCursorResult)));
    }

    [Fact]
    public void Body_has_every_member_but_status_and_problem()
    {
        var body = Body(new TestCursorResult(Items, "abc"));

        Assert.Equal(["data", "nextCursor"], body.Select(p => p.Key).OrderBy(k => k));
        Assert.Equal("abc", body["nextCursor"]!.GetValue<string>());
        Assert.Equal(2, body["data"]!.AsArray().Count);
    }

    [Fact]
    public void JsonPropertyName_is_honored()
    {
        var body = Body(new RenamedCursorResult(Items));

        Assert.True(body.ContainsKey("items"));
        Assert.False(body.ContainsKey("data"));
    }

    [Fact]
    public void Null_data_is_written_as_null()
    {
        var body = Body(new TestCursorResult(null, "abc"));

        Assert.True(body.ContainsKey("data"));
        Assert.Null(body["data"]);
    }

    [Fact]
    public void Problem_wins_and_204_has_no_body()
    {
        var problem = new ProblemBuilder(404).WithTitle("Missing").Build();

        Assert.Same(problem, ResultResponseBody.GetBody(new TestCursorResult(Items, null, problem: problem), Web));
        Assert.Null(ResultResponseBody.GetBody(new TestCursorResult(Items, null, status: 204), Web));
    }

    [Fact]
    public void Non_envelopes_keep_todays_body()
    {
        var typed = new Result<CursorItem>(new CursorItem { Id = 3 });
        var list = new ListResult<CursorItem>(Items);

        Assert.Same(ResultResponseBody.GetBody(typed), ResultResponseBody.GetBody(typed, Web));
        Assert.Same(ResultResponseBody.GetBody(list), ResultResponseBody.GetBody(list, Web));
        Assert.Null(ResultResponseBody.GetBody(Results.Ok(), Web));
    }

    [Fact]
    public void Without_options_an_envelope_is_returned_as_is()
        => Assert.IsType<TestCursorResult>(ResultResponseBody.GetBody(new TestCursorResult(Items, "abc")));
}
