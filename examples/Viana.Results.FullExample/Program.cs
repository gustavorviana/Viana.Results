using Viana.Results.Mediators;
using Viana.Results.Mvc;
using Viana.Results.OpenApi.Swashbuckle;

var builder = WebApplication.CreateBuilder(args);

// Viana.Results.Mvc: converte ações que retornam IResult/Result<T> em JSON e status HTTP
builder.Services.AddControllers()
    .AddVianaResultFilter();

// Viana.Results.Mediators: registra IMediator e todos os IHandler<TRequest,TResult> do assembly
builder.Services.AddMediator(typeof(Program).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.AddVianaResultFilters());

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
