using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests;

/// <summary>Feature 011: Results.Ok() without a value answers 204 with no body; an explicit status is kept.</summary>
public class BodylessSuccessTests
{
    [Theory]
    [InlineData("/success/ok-empty", 204)]
    [InlineData("/m/success/ok-empty", 204)]
    [InlineData("/f/ok-empty", 204)]
    [InlineData("/success/explicit-200", 200)]
    [InlineData("/m/success/explicit-200", 200)]
    public async Task Bodyless_success_answers_its_status_without_body(string url, int status)
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { AddProblemDetails = true, Endpoints = MinimalEndpoints.Map });

        var response = await app.Client.GetAsync(url);

        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
    }
}
