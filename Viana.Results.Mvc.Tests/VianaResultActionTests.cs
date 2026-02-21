using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Viana.Results.Mvc.Tests;

public class VianaResultActionTests
{
    private static ActionContext CreateActionContext(IServiceProvider? requestServices = null)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = requestServices ?? new ServiceCollection().BuildServiceProvider(),
            Response = { Body = new MemoryStream() }
        };
        return new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
    }

    private static string ReadResponseBody(ActionContext context)
    {
        context.HttpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(context.HttpContext.Response.Body, Encoding.UTF8, leaveOpen: true);
        return reader.ReadToEnd();
    }

    [Fact]
    public async Task ExecuteResultAsync_SetsStatusCode_FromResultStatus()
    {
        var result = Results.Ok("ok");
        var action = new VianaResultAction(result);
        var context = CreateActionContext();

        await action.ExecuteResultAsync(context);

        Assert.Equal(200, context.HttpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ExecuteResultAsync_WithProblem_SetsStatusCodeAndWritesProblemJson()
    {
        var problem = ProblemResult.WithDescription(422, "Validation Error", "Invalid input");
        var result = new Result(problem);
        var action = new VianaResultAction(result);
        var context = CreateActionContext();

        await action.ExecuteResultAsync(context);

        Assert.Equal(422, context.HttpContext.Response.StatusCode);
        Assert.Equal("application/json", context.HttpContext.Response.ContentType);
        var body = ReadResponseBody(context);
        var json = JsonDocument.Parse(body);
        Assert.Equal(422, json.RootElement.GetProperty("Status").GetInt32());
        Assert.Equal("Validation Error", json.RootElement.GetProperty("Title").GetString());
    }

    [Fact]
    public async Task ExecuteResultAsync_WithSuccessData_WritesDataJson()
    {
        var result = Results.Ok(new { Id = 1, Name = "Test" });
        var action = new VianaResultAction(result);
        var context = CreateActionContext();

        await action.ExecuteResultAsync(context);

        Assert.Equal(200, context.HttpContext.Response.StatusCode);
        Assert.Equal("application/json", context.HttpContext.Response.ContentType);
        var body = ReadResponseBody(context);
        var json = JsonDocument.Parse(body);
        Assert.Equal(1, json.RootElement.GetProperty("Id").GetInt32());
        Assert.Equal("Test", json.RootElement.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task ExecuteResultAsync_WithNoContent_DoesNotWriteBody()
    {
        var result = Results.NoContent();
        var action = new VianaResultAction(result);
        var context = CreateActionContext();

        await action.ExecuteResultAsync(context);

        Assert.Equal(204, context.HttpContext.Response.StatusCode);
        var body = ReadResponseBody(context);
        Assert.Empty(body);
    }

    [Fact]
    public async Task ExecuteResultAsync_WritesCustomHeaders_ExcludingContentType()
    {
        var result = Results.Ok("data");
        var action = new VianaResultAction(result);
        action.Headers["X-Custom"] = "value";
        action.Headers["Content-Type"] = "ignored";
        var context = CreateActionContext();

        await action.ExecuteResultAsync(context);

        Assert.True(context.HttpContext.Response.Headers.TryGetValue("X-Custom", out var custom));
        Assert.Equal("value", custom);
        Assert.Equal("application/json", context.HttpContext.Response.ContentType);
    }

    [Fact]
    public async Task ExecuteResultAsync_UsesJsonOptionsFromRequestServices_WhenAvailable()
    {
        var jsonOptions = new JsonOptions();
        jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        var services = new ServiceCollection();
        services.AddSingleton<IOptions<JsonOptions>>(Options.Create(jsonOptions));
        var provider = services.BuildServiceProvider();

        var result = Results.Ok(new { SomeProperty = "value" });
        var action = new VianaResultAction(result);
        var context = CreateActionContext(provider);

        await action.ExecuteResultAsync(context);

        var body = ReadResponseBody(context);
        Assert.Contains("someProperty", body);
        Assert.Contains("value", body);
    }

    [Fact]
    public async Task ExecuteResultAsync_ListResult_WithProblem_SetsStatusCodeAndWritesProblemJson()
    {
        var problem = ProblemResult.WithDescription(400, "Bad Request", "Invalid list request");
        ListResult<string> result = problem;
        var action = new VianaResultAction(result);
        var context = CreateActionContext();

        await action.ExecuteResultAsync(context);

        Assert.Equal(400, context.HttpContext.Response.StatusCode);
        Assert.Equal("application/json", context.HttpContext.Response.ContentType);
        var body = ReadResponseBody(context);
        var json = JsonDocument.Parse(body);
        Assert.Equal(400, json.RootElement.GetProperty("Status").GetInt32());
        Assert.Equal("Bad Request", json.RootElement.GetProperty("Title").GetString());
    }

    [Fact]
    public async Task ExecuteResultAsync_ListResult_WithSuccess_WritesFullListResultJson()
    {
        var items = new List<string> { "a", "b", "c" };
        var result = new ListResult<string>(items);
        var action = new VianaResultAction(result);
        var context = CreateActionContext();

        await action.ExecuteResultAsync(context);

        Assert.Equal(200, context.HttpContext.Response.StatusCode);
        Assert.Equal("application/json", context.HttpContext.Response.ContentType);
        var body = ReadResponseBody(context);
        var json = JsonDocument.Parse(body);
        Assert.True(json.RootElement.TryGetProperty("Data", out var dataProp));
        Assert.Equal(JsonValueKind.Array, dataProp.ValueKind);
        Assert.Equal(3, dataProp.GetArrayLength());
        Assert.Equal("a", dataProp[0].GetString());
        Assert.Equal("b", dataProp[1].GetString());
        Assert.Equal("c", dataProp[2].GetString());
        Assert.True(json.RootElement.TryGetProperty("Status", out var statusProp));
        Assert.Equal(200, statusProp.GetInt32());
    }

    [Fact]
    public async Task ExecuteResultAsync_PagedResult_WithProblem_SetsStatusCodeAndWritesProblemJson()
    {
        var problem = ProblemResult.WithDescription(404, "Not Found", "Page not found");
        PagedResult<int> result = problem;
        var action = new VianaResultAction(result);
        var context = CreateActionContext();

        await action.ExecuteResultAsync(context);

        Assert.Equal(404, context.HttpContext.Response.StatusCode);
        Assert.Equal("application/json", context.HttpContext.Response.ContentType);
        var body = ReadResponseBody(context);
        var json = JsonDocument.Parse(body);
        Assert.Equal(404, json.RootElement.GetProperty("Status").GetInt32());
        Assert.Equal("Not Found", json.RootElement.GetProperty("Title").GetString());
    }

    [Fact]
    public async Task ExecuteResultAsync_PagedResult_WithSuccess_WritesFullPagedResultJson()
    {
        var items = new List<int> { 10, 20, 30 };
        var result = PagedResult<int>.Create(items, pageNumber: 2, pageSize: 3, totalCount: 9);
        var action = new VianaResultAction(result);
        var context = CreateActionContext();

        await action.ExecuteResultAsync(context);

        Assert.Equal(200, context.HttpContext.Response.StatusCode);
        Assert.Equal("application/json", context.HttpContext.Response.ContentType);
        var body = ReadResponseBody(context);
        var json = JsonDocument.Parse(body);
        Assert.True(json.RootElement.TryGetProperty("Data", out var dataProp));
        Assert.Equal(JsonValueKind.Array, dataProp.ValueKind);
        Assert.Equal(3, dataProp.GetArrayLength());
        Assert.Equal(10, dataProp[0].GetInt32());
        Assert.Equal(20, dataProp[1].GetInt32());
        Assert.Equal(30, dataProp[2].GetInt32());
        Assert.True(json.RootElement.TryGetProperty("PageNumber", out var pageProp));
        Assert.Equal(2, pageProp.GetInt32());
        Assert.True(json.RootElement.TryGetProperty("TotalPages", out var totalProp));
        Assert.Equal(3, totalProp.GetInt32());
        Assert.True(json.RootElement.TryGetProperty("Status", out var statusProp));
        Assert.Equal(200, statusProp.GetInt32());
    }
}
