using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.OpenApi.Swashbuckle;

namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>
/// Feature 011: one real app per generator (native OpenAPI or Swashbuckle) with the controllers of this folder
/// and the minimal endpoints below; the document is read over HTTP exactly like a client.
/// </summary>
public static class DefaultResultHost
{
    public const string Native = "native";
    public const string Swashbuckle = "swashbuckle";

    public static TheoryData<string> Generators() => new() { Native, Swashbuckle };

    public static async Task<JsonElement> Document(string generator)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseSetting("urls", "http://127.0.0.1:0");
        builder.Services.AddControllers().AddApplicationPart(typeof(DefaultResultHost).Assembly);
        builder.Services.AddVianaResultExamples(o => o.AddExample<GlobalServerError>(500));

        if (generator == Native)
            builder.Services.AddOpenApi(o => o.AddVianaResultTransformers());
        else
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(o => o.AddVianaResultFilters());
        }

        await using var app = builder.Build();
        MinimalEndpoints.Map(app);
        app.MapControllers();
        if (generator == Native)
            app.MapOpenApi();
        else
            app.UseSwagger();

        await app.StartAsync();
        try
        {
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            var url = generator == Native ? "/openapi/v1.json" : "/swagger/v1/swagger.json";
            return JsonDocument.Parse(await client.GetStringAsync(url)).RootElement.Clone();
        }
        finally
        {
            await app.StopAsync();
        }
    }

    /// <summary>Response status keys of an operation, e.g. ["204", "404"].</summary>
    public static string[] Statuses(this JsonElement document, string path, string method)
        => document.GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("responses")
            .EnumerateObject().Select(p => p.Name).OrderBy(s => s).ToArray();

    public static JsonElement Response(this JsonElement document, string path, string method, string status)
        => document.GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("responses").GetProperty(status);

    public static bool HasContent(this JsonElement response)
        => response.TryGetProperty("content", out var content) && content.EnumerateObject().Any();
}
