using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.AspNetCore.Tests.TestHost;

namespace Viana.Results.AspNetCore.Tests;

public class SuccessFormattingTests
{
    [Fact]
    public async Task Value_UsesSystemTextJsonWithAppNaming()
    {
        await using var app = await TestApp.StartAsync();

        var response = await app.Client.GetAsync("/success/value");
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(22, root.GetProperty("temperatureC").GetInt32());
    }

    [Fact]
    public async Task Value_UsesNewtonsoftWhenConfigured()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { Mvc = mvc => mvc.AddNewtonsoftJson() });

        var body = await app.Client.GetStringAsync("/success/value");

        Assert.Contains("\"renamed\":\"Mild\"", body);
    }

    [Fact]
    public async Task Value_HonorsContentNegotiation()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions { Mvc = mvc => mvc.AddXmlSerializerFormatters() });
        var request = new HttpRequestMessage(HttpMethod.Get, "/success/value");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

        var response = await app.Client.SendAsync(request);

        Assert.Equal("application/xml", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("<TemperatureC>22</TemperatureC>", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Status201_IsKept()
    {
        await using var app = await TestApp.StartAsync();

        Assert.Equal(201, (int)(await app.Client.GetAsync("/success/created")).StatusCode);
    }

    [Fact]
    public async Task NoContent_WritesNoBody()
    {
        await using var app = await TestApp.StartAsync();

        var response = await app.Client.GetAsync("/success/no-content");

        Assert.Equal(204, (int)response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task NullValue_FollowsMvcDefault_204()
    {
        await using var app = await TestApp.StartAsync();

        var response = await app.Client.GetAsync("/success/null-value");

        Assert.Equal(204, (int)response.StatusCode);
    }

    [Fact]
    public async Task NullValue_WithTreatNullAsNoContentDisabled_WritesNull()
    {
        await using var app = await TestApp.StartAsync(new TestAppOptions
        {
            Mvc = mvc => mvc.AddMvcOptions(o =>
            {
                foreach (var f in o.OutputFormatters.OfType<HttpNoContentOutputFormatter>())
                    f.TreatNullValueAsNoContent = false;
            })
        });

        var response = await app.Client.GetAsync("/success/null-value");

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("null", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ListResult_IsJsonArray()
    {
        await using var app = await TestApp.StartAsync();

        var root = JsonDocument.Parse(await app.Client.GetStringAsync("/success/list")).RootElement;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.Equal(2, root.GetArrayLength());
    }

    [Fact]
    public async Task PagedResult_KeepsMetadata()
    {
        await using var app = await TestApp.StartAsync();

        var root = JsonDocument.Parse(await app.Client.GetStringAsync("/success/paged")).RootElement;

        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal(3, root.GetProperty("totalPages").GetInt32());
    }
}
