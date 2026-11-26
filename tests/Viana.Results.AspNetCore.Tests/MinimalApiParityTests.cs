using System.Text.Json;
using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests;

/// <summary>FR-003 / SC-001: the same result answers identically from a controller and from a minimal API endpoint.</summary>
public class MinimalApiParityTests
{
    public static TheoryData<string, bool> Paths()
    {
        var paths = new[]
        {
            "cases/explicit", "cases/trace", "cases/untitled/422", "cases/non-error", "cases/status-differs",
            "cases/failed/result", "cases/failed/typed", "cases/failed/list", "cases/failed/paged",
            "success/value", "success/created", "success/no-content", "success/null-value", "success/list", "success/paged"
        };
        var data = new TheoryData<string, bool>();
        foreach (var path in paths)
        {
            data.Add(path, true);
            data.Add(path, false);
        }
        return data;
    }

    internal static async Task<(int Status, string? MediaType, string Body)> GetAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();
        if (body.Length > 0 && (body[0] == '{' || body[0] == '['))
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
                body = JsonSerializer.Serialize(doc.RootElement.EnumerateObject().Where(p => p.Name != "traceId").ToDictionary(p => p.Name, p => p.Value.ToString()));
        }
        return ((int)response.StatusCode, response.Content.Headers.ContentType?.MediaType, body);
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task MinimalEndpoint_EqualsController(string path, bool addProblemDetails)
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { AddProblemDetails = addProblemDetails, Endpoints = MinimalEndpoints.Map });

        var controller = await GetAsync(app.Client, "/" + path);
        var minimal = await GetAsync(app.Client, "/m/" + path);

        Assert.Equal(controller, minimal);
    }

    [Fact]
    public async Task NullValue_Returns204()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { Endpoints = MinimalEndpoints.Map });

        Assert.Equal(204, (await GetAsync(app.Client, "/m/success/null-value")).Status);
    }
}
