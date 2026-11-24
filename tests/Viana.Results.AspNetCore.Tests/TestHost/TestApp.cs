using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Viana.Results.AspNetCore.Tests.TestHost;

/// <summary>Starts an in-memory ASP.NET Core app with the test controllers of this assembly.</summary>
public sealed class TestApp : IAsyncDisposable
{
    private readonly WebApplication _app;

    private TestApp(WebApplication app, HttpClient client)
    {
        _app = app;
        Client = client;
    }

    /// <summary>Client bound to the in-memory server.</summary>
    public HttpClient Client { get; }

    /// <summary>Number of times <c>CustomizeProblemDetails</c> ran.</summary>
    public int CustomizeCalls => _customizeCalls;

    private int _customizeCalls;

    /// <summary>Creates and starts the application.</summary>
    public static async Task<TestApp> StartAsync(TestAppOptions? options = null)
    {
        options ??= new TestAppOptions();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        TestApp? instance = null;
        if (options.AddProblemDetails)
        {
            builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
            {
                Interlocked.Increment(ref instance!._customizeCalls);
                options.Customize?.Invoke(ctx);
            });
        }

        var mvc = builder.Services.AddControllers().AddApplicationPart(typeof(TestApp).Assembly);
        options.Mvc?.Invoke(mvc);
        options.Services?.Invoke(builder.Services);

        var app = builder.Build();
        app.MapControllers();
        options.Endpoints?.Invoke(app);

        await app.StartAsync();
        instance = new TestApp(app, app.GetTestClient());
        return instance;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }
}
