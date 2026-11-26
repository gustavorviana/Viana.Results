using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using Viana.Results.OpenApi.Swashbuckle;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>
/// Feature 013: builds an app with only the given controllers and minimal endpoints and generates its document on
/// the native pipeline or on Swashbuckle, in process.
/// </summary>
public static class ValidationHost
{
    public const string Native = "native";
    public const string Swashbuckle = "swashbuckle";

    public static TheoryData<string> Generators() => new() { Native, Swashbuckle };

    public static async Task<OpenApiDocument> Generate(string generator, Type[] controllers, Action<WebApplication>? map = null, ListLoggerProvider? logs = null)
    {
        var builder = WebApplication.CreateBuilder();
        if (logs != null)
            builder.Logging.AddProvider(logs);
        builder.Services.AddControllers()
            .ConfigureApplicationPartManager(m => m.FeatureProviders.Add(new OnlyControllers(controllers)));

        if (generator == Native)
            builder.Services.AddOpenApi(o => o.AddVianaResultTransformers());
        else
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(o => o.AddVianaResultFilters());
        }

        builder.WebHost.UseSetting("urls", "http://127.0.0.1:0");
        await using var app = builder.Build();
        map?.Invoke(app);
        app.MapControllers();

        // Minimal endpoints reach ApiExplorer once the routing pipeline is built.
        await app.StartAsync();
        try
        {
            if (generator == Native)
                return await app.Services.GetRequiredKeyedService<IOpenApiDocumentProvider>("v1").GetOpenApiDocumentAsync();

            return app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
        }
        finally
        {
            await app.StopAsync();
        }
    }

    /// <summary>The validation error, unwrapped from the generator's own exception (Swashbuckle wraps it).</summary>
    public static async Task<InvalidOperationException> Failure(string generator, Type[] controllers, Action<WebApplication>? map = null)
    {
        Exception? ex = await Record.ExceptionAsync(() => Generate(generator, controllers, map));
        while (ex != null && ex is not InvalidOperationException)
            ex = ex.InnerException;
        return Assert.IsType<InvalidOperationException>(ex);
    }

    public static string[] Statuses(this OpenApiDocument document, string path)
        => document.Paths![path].Operations!.Values.First().Responses!.Keys.OrderBy(k => k).ToArray();
}
