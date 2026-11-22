using System.Text.Json;
using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests;

/// <summary>FR-004 / SC-001: a Viana failure equals the framework's own Problem(...) response.</summary>
public class ProblemParityTests
{
    public static TheoryData<int, bool> Cases()
    {
        var data = new TheoryData<int, bool>();
        foreach (var status in new[] { 400, 401, 403, 404, 409, 422, 500 })
        {
            data.Add(status, true);
            data.Add(status, false);
        }
        return data;
    }

    private static async Task<(int Status, string? MediaType, Dictionary<string, string> Members)> GetAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var members = json.RootElement.EnumerateObject()
            .Where(p => p.Name != "traceId")
            .ToDictionary(p => p.Name, p => p.Value.ToString());
        Assert.True(json.RootElement.TryGetProperty("traceId", out _), "traceId missing");
        return ((int)response.StatusCode, response.Content.Headers.ContentType?.MediaType, members);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task VianaFailure_EqualsNativeProblem(int status, bool addProblemDetails)
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { AddProblemDetails = addProblemDetails });
        var query = "?title=Some%20title&detail=Some%20detail";

        var native = await GetAsync(app.Client, $"/parity/native/{status}{query}");
        var viana = await GetAsync(app.Client, $"/parity/viana/{status}{query}");

        Assert.Equal(native.Status, viana.Status);
        Assert.Equal(native.MediaType, viana.MediaType);
        Assert.Equal(native.Members, viana.Members);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task VianaFailure_WithExplicitType_EqualsNativeProblem(int status, bool addProblemDetails)
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { AddProblemDetails = addProblemDetails });
        var query = "?title=T&type=https%3A%2F%2Fapi.example.com%2Ferrors%2Fx";

        var native = await GetAsync(app.Client, $"/parity/native/{status}{query}");
        var viana = await GetAsync(app.Client, $"/parity/viana/{status}{query}");

        Assert.Equal(native.Members, viana.Members);
    }
}
