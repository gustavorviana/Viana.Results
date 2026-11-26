using System.Text.Json;
using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests;

public class ProblemPrecedenceTests
{
    private static async Task<(HttpResponseMessage Response, JsonElement Root)> GetAsync(TestApp app, string url)
    {
        var response = await app.Client.GetAsync(url);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
        return (response, root);
    }

    [Fact]
    public async Task ExplicitValues_AreKept_AndNamesFollowPolicy()
    {
        await using var app = await TestApp.StartAsync();

        var (response, root) = await GetAsync(app, "/cases/explicit");

        Assert.Equal(409, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Order conflict", root.GetProperty("title").GetString());
        Assert.Equal("https://api.example.com/errors/conflict", root.GetProperty("type").GetString());
        Assert.Equal("Order 1 already shipped.", root.GetProperty("detail").GetString());
        Assert.Equal("/orders/1", root.GetProperty("instance").GetString());
        Assert.Equal("E1", root.GetProperty("errorCode").GetString()); // MVC default camelCase policy
        Assert.False(root.TryGetProperty("ErrorCode", out _));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NativeTraceId_AlwaysWins_AndAppearsOnce(bool addProblemDetails)
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { AddProblemDetails = addProblemDetails });

        var response = await app.Client.GetAsync("/cases/trace");
        var body = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(body).RootElement;

        Assert.NotEqual("viana-trace", root.GetProperty("traceId").GetString());
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(body, "\"traceId\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    [Fact]
    public async Task UntitledProblem_KeepsVianaReasonPhrase()
    {
        await using var app = await TestApp.StartAsync();

        var (_, root) = await GetAsync(app, "/cases/untitled/422");

        Assert.Equal("Unprocessable Content", root.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Customize_RunsAfterVianaValues_Once()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions
        {
            Customize = ctx =>
            {
                ctx.ProblemDetails.Extensions["seenTitle"] = ctx.ProblemDetails.Title;
                ctx.ProblemDetails.Title = "Customized";
            }
        });

        var (_, root) = await GetAsync(app, "/cases/explicit");

        Assert.Equal("Order conflict", root.GetProperty("seenTitle").GetString());
        Assert.Equal("Customized", root.GetProperty("title").GetString());
        Assert.Equal(1, app.CustomizeCalls);
    }

    [Fact]
    public async Task ProblemWithNonErrorStatus_KeepsStatusAndWritesProblem()
    {
        await using var app = await TestApp.StartAsync();

        var (response, root) = await GetAsync(app, "/cases/non-error");

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("Odd", root.GetProperty("title").GetString());
    }

    [Fact]
    public async Task HttpStatus_EqualsResultStatus()
    {
        await using var app = await TestApp.StartAsync();

        var (response, root) = await GetAsync(app, "/cases/status-differs");

        Assert.Equal(409, (int)response.StatusCode);
        Assert.Equal(409, root.GetProperty("status").GetInt32());
    }

    [Theory]
    [InlineData("result")]
    [InlineData("typed")]
    [InlineData("list")]
    [InlineData("paged")]
    public async Task EveryResultKind_WithProblem_WritesProblem(string kind)
    {
        await using var app = await TestApp.StartAsync();

        var (response, root) = await GetAsync(app, "/cases/failed/" + kind);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal("x", root.GetProperty("title").GetString());
    }
}
