using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Viana.Results;
using Viana.Results.AspNetCore;
using Viana.Results.Examples.Shared;
using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;
using Viana.Results.Mediators.Example;
using Viana.Results.Mediators.Example.UseCases;
using Viana.Results.Mediators.Example.UseCases.Scenarios;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddProblemDetails();
builder.Services.AddCatalog();
builder.Services.AddOpenApi();

// ---------------------------------------------------------------------------------------------
// Mediator: scans this assembly for IHandler<,> implementations (registered as scoped; register a
// handler yourself before AddMediator to give it another lifetime). Every option is shown with its
// default; the values come from the "Mediator" section of appsettings.json so they can be changed
// without recompiling.
// ---------------------------------------------------------------------------------------------
builder.Services.AddMediator(options =>
{
    // Spans "Mediator.Send {Request}" on the ActivitySource MediatorDiagnostics.SourceName (default true).
    options.Telemetry.Tracing = config.GetValue("Mediator:Telemetry:Tracing", true);

    // Histogram viana.mediator.request.duration on the Meter MediatorDiagnostics.MeterName (default true).
    options.Telemetry.Metrics = config.GetValue("Mediator:Telemetry:Metrics", true);

    // What a thrown exception records on the span (default Full). Messages and stack traces may carry
    // sensitive data: TypeOnly keeps the exception type, None records no exception event (the span is
    // still Error and error.type is kept).
    options.Telemetry.ExceptionDetails = config.GetValue("Mediator:Telemetry:ExceptionDetails", ExceptionDetailLevel.Full);
}, typeof(MediatorsHost).Assembly);

// ---------------------------------------------------------------------------------------------
// OpenTelemetry: the mediator uses the native ActivitySource and Meter, so subscribing by name is all
// it takes. The console exporter prints spans and metrics; for a real backend replace
// AddConsoleExporter() with AddOtlpExporter() (package OpenTelemetry.Exporter.OpenTelemetryProtocol).
// ---------------------------------------------------------------------------------------------
var consoleExporter = config.GetValue("Mediator:ConsoleExporter", true);
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("viana-results-mediators-example"))
    .WithTracing(tracing =>
    {
        // The mediator span is a child of the ASP.NET Core request span. The default sampler follows the
        // parent, so the request source must be recorded too, or the mediator spans are dropped.
        tracing.AddSource("Microsoft.AspNetCore");
        tracing.AddSource(MediatorDiagnostics.SourceName);
        if (consoleExporter)
            tracing.AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddMeter(MediatorDiagnostics.MeterName);
        if (consoleExporter)
            metrics.AddConsoleExporter();
    });

var app = builder.Build();

// Unhandled exceptions (the throwing handler) become a 500 problem; bare status codes too.
// Malformed bodies in minimal APIs throw BadHttpRequestException in Development: keep its status (400).
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = ex => ex is BadHttpRequestException bad ? bad.StatusCode : StatusCodes.Status500InternalServerError
});
app.UseStatusCodePages();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => Microsoft.AspNetCore.Http.Results.Redirect("/scalar/v1")).ExcludeFromDescription();

// ---------------- Catalog use cases ----------------
var products = app.MapGroup("/products").AddVianaResults().WithTags("Products (mediator)");

products.MapGet("/", (IMediator mediator, CancellationToken ct) => mediator.SendAsync(new ListProducts(), ct));
products.MapGet("/{id:int}", (int id, IMediator mediator, CancellationToken ct) => mediator.SendAsync(new GetProduct(id), ct));
products.MapPost("/", ([FromBody] ProductInput input, IMediator mediator, CancellationToken ct) => mediator.SendAsync(new CreateProduct(input), ct));
products.MapPut("/{id:int}", (int id, [FromBody] ProductInput input, IMediator mediator, CancellationToken ct) => mediator.SendAsync(new UpdateProduct(id, input), ct));
products.MapDelete("/{id:int}", (int id, IMediator mediator, CancellationToken ct) => mediator.SendAsync(new DeleteProduct(id), ct));

// ---------------- Failure scenarios (see the console telemetry) ----------------
var scenarios = app.MapGroup("/scenarios").AddVianaResults().WithTags("Scenarios");

// 500: the exception reaches UseExceptionHandler; the span is Error with an exception event.
scenarios.MapGet("/throws", (IMediator mediator, CancellationToken ct) => mediator.SendAsync(new ThrowingRequest(), ct));

// 503 problem from the inventory dependency; 5xx results mark the span as Error.
scenarios.MapGet("/unavailable", (IMediator mediator, CancellationToken ct) => mediator.SendAsync(new UnavailableRequest(1), ct));

// The token is HttpContext.RequestAborted: cancel the call and the span gets viana.mediator.cancelled
// without being an error.
scenarios.MapGet("/slow", (int? seconds, IMediator mediator, CancellationToken ct) => mediator.SendAsync(new SlowRequest(seconds ?? 10), ct));

// No handler registered: the task fails with HandlerNotFoundException, which tells what was asked for.
scenarios.MapGet("/unregistered", async (IMediator mediator, CancellationToken ct) =>
{
    try
    {
        return await mediator.SendAsync(new UnregisteredRequest(), ct);
    }
    catch (HandlerNotFoundException ex)
    {
        return new ProblemBuilder(500)
            .WithTitle("Handler not found")
            .WithDetail($"No handler for {ex.RequestType?.Name} returning {ex.ResultType?.Name}.")
            .BuildResult();
    }
});

app.Run();
