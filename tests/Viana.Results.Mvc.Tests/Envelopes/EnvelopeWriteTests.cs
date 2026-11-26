using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Viana.Results.Mvc.Tests.Envelopes;

/// <summary>Feature 014 US1: VianaResultAction writes envelopes whole.</summary>
public class EnvelopeWriteTests
{
    private static async Task<(HttpResponse Response, string Body)> Execute(IResult result)
    {
        var http = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider(),
            Response = { Body = new MemoryStream() }
        };
        var context = new ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

        await new VianaResultAction(result).ExecuteResultAsync(context);

        http.Response.Body.Position = 0;
        using var reader = new StreamReader(http.Response.Body, Encoding.UTF8, leaveOpen: true);
        return (http.Response, await reader.ReadToEndAsync());
    }

    [Fact]
    public async Task Envelope_is_written_without_status_and_problem()
    {
        var (response, body) = await Execute(new MvcCursorResult([1, 2], "abc"));

        Assert.Equal(200, response.StatusCode);
        var root = JsonDocument.Parse(body).RootElement;
        Assert.Equal(2, root.GetProperty("Data").GetArrayLength());
        Assert.Equal("abc", root.GetProperty("NextCursor").GetString());
        Assert.False(root.TryGetProperty("Status", out _));
        Assert.False(root.TryGetProperty("Problem", out _));
    }

    [Fact]
    public async Task Problem_envelope_writes_the_problem()
    {
        var (response, body) = await Execute(new MvcCursorResult([], null, problem: new ProblemBuilder(404).WithTitle("Missing").Build()));

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("application/problem+json", response.ContentType);
        Assert.Equal("Missing", JsonDocument.Parse(body).RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Status_204_has_no_body()
    {
        var (response, body) = await Execute(new MvcCursorResult([1], "abc", status: 204));

        Assert.Equal(204, response.StatusCode);
        Assert.Empty(body);
    }
}
