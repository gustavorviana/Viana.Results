using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Viana.Results.OpenApi.Swashbuckle;

namespace Viana.Results.OpenApi.Tests.Swashbuckle;

/// <summary>Feature 008 T012: minimal API declarations on the sync (GetSwagger) and async (GetSwaggerAsync) paths.</summary>
public class SwaggerPathsTests
{
    private static async Task<WebApplication> BuildApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseSetting("urls", "http://127.0.0.1:0");
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o => o.AddVianaResultFilters());
        var app = builder.Build();
        // Feature 013: Viana declarations require a Viana result return.
        app.MapGet("/users/{id}", (int id) => Results.Ok())
           .ProducesProblemResult(404, title: "User not found");
        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task AsyncPath_DocumentsMinimalDeclarations()
    {
        await using var app = await BuildApp();
        var provider = app.Services.GetRequiredService<IAsyncSwaggerProvider>();

        var document = await provider.GetSwaggerAsync("v1");

        var operation = document.Paths!["/users/{id}"].Operations!.Values.Single();
        Assert.True(operation.Responses!.ContainsKey("404"));
    }

    [Fact]
    public async Task SyncPath_DocumentsMinimalDeclarations()
    {
        await using var app = await BuildApp();
        var provider = app.Services.GetRequiredService<ISwaggerProvider>();

        var document = provider.GetSwagger("v1");

        var operation = document.Paths!["/users/{id}"].Operations!.Values.Single();
        Assert.True(operation.Responses!.ContainsKey("404"));
    }
}
