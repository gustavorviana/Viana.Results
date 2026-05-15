using Scalar.AspNetCore;
using Viana.Results.Examples.Shared.Controllers;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.Mediators;
using Viana.Results.Mvc;
using Viana.Results.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var sharedAssembly = typeof(ResultSamplesController).Assembly;

// Viana.Results.Mvc converts actions that return IResult/Result<T> into JSON + HTTP status.
// Controllers and handlers live in Viana.Results.Examples.Shared.
builder.Services.AddControllers()
    .AddApplicationPart(sharedAssembly)
    .AddVianaResultFilter();

builder.Services.AddMediator(sharedAssembly);

builder.Services.AddVianaResultExamples(opts =>
{
    opts.AddExample<InternalServerErrorExample>(500, summary: "Generic internal error");
});

builder.Services.AddOpenApi(options =>
    options.AddVianaResultTransformers());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/scalar/v1");
        return;
    }
    await next();
});

app.MapControllers();

app.Run();
