using System.Text.Json;
using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests;

public class MinimalApiFilterTests
{
    private static Task<TestApp> StartAsync() => TestApp.StartAsync(new TestAppOptions { Endpoints = MinimalEndpoints.Map });

    [Fact]
    public async Task GroupFilter_DirectSuccess_EqualsExplicit()
    {
        await using var app = await StartAsync();

        Assert.Equal(await MinimalApiParityTests.GetAsync(app.Client, "/explicit/ok"), await MinimalApiParityTests.GetAsync(app.Client, "/f/ok"));
    }

    [Fact]
    public async Task EndpointFilter_DirectSuccess_EqualsExplicit()
    {
        await using var app = await StartAsync();

        Assert.Equal(await MinimalApiParityTests.GetAsync(app.Client, "/explicit/ok"), await MinimalApiParityTests.GetAsync(app.Client, "/single/ok"));
    }

    [Fact]
    public async Task GroupFilter_DirectAsyncFailure_EqualsExplicit()
    {
        await using var app = await StartAsync();

        var direct = await MinimalApiParityTests.GetAsync(app.Client, "/f/fail");

        Assert.Equal(await MinimalApiParityTests.GetAsync(app.Client, "/explicit/fail"), direct);
        Assert.Equal("application/problem+json", direct.MediaType);
    }

    [Fact]
    public async Task NonVianaValues_AreUnchanged()
    {
        await using var app = await StartAsync();

        Assert.Equal("x", (await MinimalApiParityTests.GetAsync(app.Client, "/f/plain")).Body);
    }

    [Fact]
    public async Task MinimalJsonNaming_IsHonored()
    {
        await using var app = await StartAsync();

        var ok = JsonDocument.Parse((await app.Client.GetStringAsync("/m/success/value"))).RootElement;
        var fail = JsonDocument.Parse(await (await app.Client.GetAsync("/m/cases/explicit")).Content.ReadAsStringAsync()).RootElement;

        Assert.True(ok.TryGetProperty("temperatureC", out _));
        Assert.True(fail.TryGetProperty("errorCode", out _));
    }
}
