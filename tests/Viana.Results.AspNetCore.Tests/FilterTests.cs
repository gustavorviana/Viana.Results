using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests;

public class FilterTests
{
    private static async Task<(int Status, string? MediaType, string Body)> GetAsync(TestApp app, string url)
    {
        var response = await app.Client.GetAsync(url);
        return ((int)response.StatusCode, response.Content.Headers.ContentType?.MediaType, await response.Content.ReadAsStringAsync());
    }

    private static string WithoutTraceId(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc.RootElement.EnumerateObject().Where(p => p.Name != "traceId").ToDictionary(p => p.Name, p => p.Value.ToString()));
    }

    [Fact]
    public async Task DirectSuccess_EqualsExplicitConversion()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { Mvc = mvc => mvc.AddVianaResultFilter() });

        Assert.Equal(await GetAsync(app, "/direct/ok-explicit"), await GetAsync(app, "/direct/ok"));
    }

    [Fact]
    public async Task DirectAsyncFailure_EqualsExplicitConversion()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { Mvc = mvc => mvc.AddVianaResultFilter() });

        var direct = await GetAsync(app, "/direct/fail");
        var explicitResult = await GetAsync(app, "/direct/fail-explicit");

        Assert.Equal(explicitResult.Status, direct.Status);
        Assert.Equal("application/problem+json", direct.MediaType);
        Assert.Equal(WithoutTraceId(explicitResult.Body), WithoutTraceId(direct.Body));
    }

    [Fact]
    public async Task NonResultValues_AreUnchanged()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { Mvc = mvc => mvc.AddVianaResultFilter() });

        Assert.Equal("x", (await GetAsync(app, "/direct/plain")).Body);
        Assert.Equal(9, JsonDocument.Parse((await GetAsync(app, "/direct/object")).Body).RootElement.GetProperty("temperatureC").GetInt32());
    }

    [Fact]
    public async Task WithoutFilter_ExplicitConversionStillWorks()
    {
        await using var app = await TestApp.StartAsync();

        var response = await GetAsync(app, "/direct/fail-explicit");

        Assert.Equal(404, response.Status);
        Assert.Equal("application/problem+json", response.MediaType);
    }

    [Fact]
    public void AddVianaResultFilter_CalledTwice_RegistersOnce()
    {
        var services = new ServiceCollection();
        var mvc = services.AddControllers();

        mvc.AddVianaResultFilter();
        mvc.AddVianaResultFilter();

        using var provider = services.BuildServiceProvider();
        var filters = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.MvcOptions>>().Value.Filters;
        Assert.Single(filters.OfType<Microsoft.AspNetCore.Mvc.ServiceFilterAttribute>(), f => f.ServiceType == typeof(Filters.VianaResultFilter));
    }
}
