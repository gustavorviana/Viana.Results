using System.Text.Json;
using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests;

/// <summary>
/// Feature 008 FR-011 (runtime side): the type sent for a problem without its own type. The OpenAPI tests
/// (ProblemTypeDefaultsTests) assert the documented type against the same links.
/// </summary>
public class RuntimeProblemTypeTests
{
    [Theory]
    [InlineData(404, "https://tools.ietf.org/html/rfc9110#section-15.5.5", true)]
    [InlineData(409, "https://tools.ietf.org/html/rfc9110#section-15.5.10", true)]
    [InlineData(500, "https://tools.ietf.org/html/rfc9110#section-15.6.1", true)]
    [InlineData(404, "https://tools.ietf.org/html/rfc9110#section-15.5.5", false)]
    [InlineData(409, "https://tools.ietf.org/html/rfc9110#section-15.5.10", false)]
    [InlineData(500, "https://tools.ietf.org/html/rfc9110#section-15.6.1", false)]
    public async Task ProblemWithoutType_SendsNativeLink(int status, string link, bool addProblemDetails)
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { AddProblemDetails = addProblemDetails, Endpoints = MinimalEndpoints.Map });

        foreach (var url in new[] { $"/cases/untitled/{status}", $"/m/cases/untitled/{status}" })
        {
            using var doc = JsonDocument.Parse(await (await app.Client.GetAsync(url)).Content.ReadAsStringAsync());
            Assert.Equal(link, doc.RootElement.GetProperty("type").GetString());
        }
    }
}
