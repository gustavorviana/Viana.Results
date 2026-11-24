using Viana.Results.AspNetCore;
using Viana.Results.Examples.Shared;
using Viana.Results.Examples.Shared.Controllers;
using Viana.Results.Examples.Shared.Endpoints;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.OpenApi;
using Viana.Results.OpenApi.Swashbuckle;

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

// Swashbuckle document (/swagger/v1/swagger.json) with the Viana filters and the XML comments.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{sharedAssembly.GetName().Name}.xml");
    if (File.Exists(xmlFile))
        options.IncludeXmlComments(xmlFile);

    options.AddVianaResultFilters();
});

var app = builder.Build();

// Malformed bodies in minimal APIs throw BadHttpRequestException in Development: keep its status (400).
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = ex => ex is BadHttpRequestException bad ? bad.StatusCode : StatusCodes.Status500InternalServerError
});
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Microsoft.AspNetCore.Http.Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();
app.MapProductEndpoints();

app.Run();
