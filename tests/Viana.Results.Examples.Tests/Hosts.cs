using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Viana.Results.Mediators.Example;
using Viana.Results.Scalar;
using Viana.Results.Swagger;

namespace Viana.Results.Examples.Tests;

/// <summary>Creates a fresh host per test, so the in-memory catalog starts from its seed every time.</summary>
public static class Hosts
{
    public const string Scalar = "scalar";
    public const string Swagger = "swagger";

    public static HttpClient Create(string host, params (string Key, string Value)[] settings) => host switch
    {
        Scalar => Configure(new WebApplicationFactory<ScalarHost>(), settings).CreateClient(),
        Swagger => Configure(new WebApplicationFactory<SwaggerHost>(), settings).CreateClient(),
        _ => throw new ArgumentOutOfRangeException(nameof(host))
    };

    public static HttpClient CreateMediators(params (string Key, string Value)[] settings)
        => Configure(new WebApplicationFactory<MediatorsHost>(), [("Mediator:ConsoleExporter", "false"), .. settings]).CreateClient();

    private static WebApplicationFactory<T> Configure<T>(WebApplicationFactory<T> factory, (string Key, string Value)[] settings) where T : class
        => factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Development");
            foreach (var (key, value) in settings)
                b.UseSetting(key, value);
        });

    public static async Task<JsonElement> Json(this HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<JsonElement>();

    public static object ValidProduct(string sku = "HS-001") => new { sku, name = "Headset", price = 499.0, stock = 5 };
}
