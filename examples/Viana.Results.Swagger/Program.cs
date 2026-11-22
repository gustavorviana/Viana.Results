using Viana.Results.Examples.Shared.Controllers;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.Mediators;
using Viana.Results.AspNetCore;
using Viana.Results.OpenApi;
using Viana.Results.OpenApi.Swashbuckle;

var builder = WebApplication.CreateBuilder(args);

var sharedAssembly = typeof(ResultSamplesController).Assembly;

// Viana.Results.AspNetCore converts actions that return IResult/Result<T> through the native pipeline
// (ProblemDetails for failures, MVC formatters for successes).
builder.Services.AddProblemDetails();
// Controllers and handlers live in Viana.Results.Examples.Shared.
builder.Services.AddControllers()
    .AddApplicationPart(sharedAssembly)
    .AddVianaResultFilter();

builder.Services.AddMediator(sharedAssembly);

builder.Services.AddVianaResultExamples(opts =>
{
    opts.AddExample<InternalServerErrorExample>(500, summary: "Generic internal error");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{sharedAssembly.GetName().Name}.xml");
    if (File.Exists(xmlFile))
        options.IncludeXmlComments(xmlFile);

    options.AddVianaResultFilters();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }
    await next();
});

app.MapControllers();

app.Run();
