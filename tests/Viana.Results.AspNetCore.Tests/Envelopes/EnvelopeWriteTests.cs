using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests.Envelopes;

/// <summary>Feature 014 US1/US2: envelopes are written whole by controllers and minimal APIs.</summary>
public class EnvelopeWriteTests
{
    private static void Endpoints(WebApplication app)
    {
        app.MapGet("/envelope/http", () => EnvelopeController.Sample().ToHttpResult());
        app.MapGroup("/envelope/group").AddVianaResults().MapGet("/direct", () => EnvelopeController.Sample());
    }

    private static void AssertEnvelope(JsonElement root)
    {
        Assert.Equal(2, root.GetProperty("items").GetArrayLength());
        Assert.Equal("abc", root.GetProperty("nextCursor").GetString());
        Assert.Equal("eu", root.GetProperty("region").GetString());
        Assert.False(root.TryGetProperty("status", out _));
        Assert.False(root.TryGetProperty("problem", out _));
        Assert.False(root.TryGetProperty("extensions", out _));
        Assert.False(root.TryGetProperty("data", out _));
    }

    [Theory]
    [InlineData("/envelope/action")]
    [InlineData("/envelope/direct")]
    [InlineData("/envelope/http")]
    [InlineData("/envelope/group/direct")]
    public async Task Envelope_is_written_whole(string path)
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions
        {
            Mvc = mvc => mvc.AddVianaResultFilter(),
            Endpoints = Endpoints,
        });

        var root = JsonDocument.Parse(await app.Client.GetStringAsync(path)).RootElement;

        AssertEnvelope(root);
    }

    [Fact]
    public async Task Naming_policy_of_the_app_applies()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions
        {
            Mvc = mvc => mvc.AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower),
        });

        var root = JsonDocument.Parse(await app.Client.GetStringAsync("/envelope/action")).RootElement;

        Assert.Equal("abc", root.GetProperty("next_cursor").GetString());
        Assert.True(root.TryGetProperty("items", out _));
    }

    [Fact]
    public async Task Newtonsoft_only_writes_the_instance()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { Mvc = mvc => mvc.AddNewtonsoftJson() });

        var root = JsonDocument.Parse(await app.Client.GetStringAsync("/envelope/action")).RootElement;

        Assert.Equal("abc", root.GetProperty("nextCursor").GetString());
    }
}
