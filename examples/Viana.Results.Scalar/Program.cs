using Scalar.AspNetCore;
using Viana.Results.AspNetCore;
using Viana.Results.Examples.Shared;
using Viana.Results.Examples.Shared.Controllers;
using Viana.Results.Examples.Shared.Endpoints;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var sharedAssembly = typeof(ProductsController).Assembly;

// Native problem details: Viana failures, unhandled exceptions and bare status codes share one format.
builder.Services.AddProblemDetails();

// Controllers live in Viana.Results.Examples.Shared. AddVianaResultFilter (Viana.Results.AspNetCore)
// converts actions returning Result/Result<T>. The Viana.Results.Mvc controller needs no registration.
builder.Services.AddControllers()
    .AddApplicationPart(sharedAssembly)
    .AddVianaResultFilter();

builder.Services.AddCatalog();

// Global example: used for 500 wherever an endpoint does not document its own.
builder.Services.AddVianaResultExamples(opts =>
    opts.AddExample<InternalServerErrorExample>(500, summary: "Generic internal error"));

// Native OpenAPI document (/openapi/v1.json) with the Viana transformers.
builder.Services.AddOpenApi(options => options.AddVianaResultTransformers());

var app = builder.Build();

// Malformed bodies in minimal APIs throw BadHttpRequestException in Development: keep its status (400).
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = ex => ex is BadHttpRequestException bad ? bad.StatusCode : StatusCodes.Status500InternalServerError
});
app.UseStatusCodePages();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => Microsoft.AspNetCore.Http.Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapControllers();
app.MapProductEndpoints();

app.Run();

namespace Viana.Results.Scalar
{
    /// <summary>Marker type for <c>WebApplicationFactory</c> (each example host has its own).</summary>
    public sealed class ScalarHost;
}
