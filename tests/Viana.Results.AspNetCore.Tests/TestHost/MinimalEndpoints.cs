using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Viana.Results.AspNetCore.Tests.TestHost;

/// <summary>Minimal API mirrors of <see cref="CasesController"/> and <see cref="SuccessController"/> (feature 006 cases).</summary>
public static class MinimalEndpoints
{
    public static void Map(WebApplication app)
    {
        var m = app.MapGroup("/m");

        m.MapGet("/cases/explicit", () => new ProblemBuilder(409)
            .WithTitle("Order conflict")
            .WithType("https://api.example.com/errors/conflict")
            .WithDetail("Order 1 already shipped.")
            .WithInstance("/orders/1")
            .AddExtension("ErrorCode", "E1")
            .BuildResult()
            .ToHttpResult());
        m.MapGet("/cases/trace", () => new ProblemBuilder(400).AddExtension("TraceId", "viana-trace").BuildResult().ToHttpResult());
        m.MapGet("/cases/untitled/{status:int}", (int status) => new ProblemBuilder(status).BuildResult().ToHttpResult());
        m.MapGet("/cases/non-error", () => new Result(200, new ProblemResult(200, "Odd")).ToHttpResult());
        m.MapGet("/cases/status-differs", () => new Result(409, new ProblemResult(400, "x")).ToHttpResult());
        m.MapGet("/cases/failed/result", () => new Result(new ProblemResult(404, "x")).ToHttpResult());
        m.MapGet("/cases/failed/typed", () => new Result<Forecast>(new ProblemResult(404, "x")).ToHttpResult());
        m.MapGet("/cases/failed/list", () => ((ListResult<Forecast>)new ProblemResult(404, "x")).ToHttpResult());
        m.MapGet("/cases/failed/paged", () => ((PagedResult<Forecast>)new ProblemResult(404, "x")).ToHttpResult());

        m.MapGet("/success/value", () => Results.Ok(new Forecast { TemperatureC = 22, Summary = "Mild" }).ToHttpResult());
        m.MapGet("/success/created", () => new Result<Forecast>(new Forecast { TemperatureC = 1 }, 201).ToHttpResult());
        m.MapGet("/success/no-content", () => new Result(204).ToHttpResult());
        m.MapGet("/success/null-value", () => new Result<Forecast>((Forecast?)null).ToHttpResult());
        m.MapGet("/success/list", () => new ListResult<Forecast>(new List<Forecast> { new() { TemperatureC = 1 }, new() { TemperatureC = 2 } }).ToHttpResult());
        m.MapGet("/success/paged", () => new PagedResult<Forecast>(new List<Forecast> { new() { TemperatureC = 1 } }, 1, 3).ToHttpResult());

        // Endpoint filter: handlers return Viana results directly.
        var f = app.MapGroup("/f").AddVianaResults();
        f.MapGet("/ok", () => Results.Ok(new Forecast { TemperatureC = 5 }));
        f.MapGet("/fail", async () => { await Task.Yield(); return new Result<Forecast>(new ProblemResult(404, "Missing")); });
        f.MapGet("/plain", () => "x");
        app.MapGet("/single/ok", () => Results.Ok(new Forecast { TemperatureC = 5 })).AddVianaResults();
        app.MapGet("/explicit/ok", () => Results.Ok(new Forecast { TemperatureC = 5 }).ToHttpResult());
        app.MapGet("/explicit/fail", () => new Result<Forecast>(new ProblemResult(404, "Missing")).ToHttpResult());
    }
}
