using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.AspNetCore.Tests.TestHost;

public sealed class Forecast
{
    public int TemperatureC { get; set; }

    [Newtonsoft.Json.JsonProperty("renamed")]
    public string? Summary { get; set; }
}

[ApiController]
[Route("parity")]
public sealed class ParityController : ControllerBase
{
    [HttpGet("native/{status:int}")]
    public IActionResult Native(int status, string? title, string? type, string? detail)
        => Problem(statusCode: status, title: title, type: type, detail: detail);

    [HttpGet("viana/{status:int}")]
    public IActionResult Viana(int status, string? title, string? type, string? detail)
    {
        var builder = new ProblemBuilder(status).WithTitle(title).WithType(type);
        if (detail != null)
            builder.WithDetail(detail);
        return builder.BuildResult().ToActionResult();
    }
}

[ApiController]
[Route("cases")]
public sealed class CasesController : ControllerBase
{
    [HttpGet("explicit")]
    public IActionResult Explicit() => new ProblemBuilder(409)
        .WithTitle("Order conflict")
        .WithType("https://api.example.com/errors/conflict")
        .WithDetail("Order 1 already shipped.")
        .WithInstance("/orders/1")
        .AddExtension("ErrorCode", "E1")
        .BuildResult()
        .ToActionResult();

    [HttpGet("trace")]
    public IActionResult Trace() => new ProblemBuilder(400).AddExtension("TraceId", "viana-trace").BuildResult().ToActionResult();

    [HttpGet("untitled/{status:int}")]
    public IActionResult Untitled(int status) => new ProblemBuilder(status).BuildResult().ToActionResult();

    [HttpGet("non-error")]
    public IActionResult NonError() => new Result(200, new ProblemResult(200, "Odd")).ToActionResult();

    [HttpGet("status-differs")]
    public IActionResult StatusDiffers() => new Result(409, new ProblemResult(400, "x")).ToActionResult();

    [HttpGet("failed/result")]
    public IActionResult FailedResult() => new Result(new ProblemResult(404, "x")).ToActionResult();

    [HttpGet("failed/typed")]
    public IActionResult FailedTyped() => new Result<Forecast>(new ProblemResult(404, "x")).ToActionResult();

    [HttpGet("failed/list")]
    public IActionResult FailedList() => ((ListResult<Forecast>)new ProblemResult(404, "x")).ToActionResult();

    [HttpGet("failed/paged")]
    public IActionResult FailedPaged() => ((PagedResult<Forecast>)new ProblemResult(404, "x")).ToActionResult();

    [HttpGet("async")]
    public Task<IActionResult> Async() => Task.FromResult<IResult>(Results.Ok(new Forecast { TemperatureC = 1 })).ToActionResult();
}

[ApiController]
[Route("success")]
public sealed class SuccessController : ControllerBase
{
    [HttpGet("value")]
    public IActionResult Value() => Results.Ok(new Forecast { TemperatureC = 22, Summary = "Mild" }).ToActionResult();

    [HttpGet("created")]
    public IActionResult Created201() => new Result<Forecast>(new Forecast { TemperatureC = 1 }, 201).ToActionResult();

    [HttpGet("no-content")]
    public IActionResult NoContent204() => new Result(204).ToActionResult();

    [HttpGet("null-value")]
    public IActionResult NullValue() => new Result<Forecast>((Forecast?)null).ToActionResult();

    [HttpGet("list")]
    public IActionResult List() => new ListResult<Forecast>(new List<Forecast> { new() { TemperatureC = 1 }, new() { TemperatureC = 2 } }).ToActionResult();

    [HttpGet("paged")]
    public IActionResult Paged() => new PagedResult<Forecast>(new List<Forecast> { new() { TemperatureC = 1 } }, 1, 3).ToActionResult();
}

[ApiController]
[Route("direct")]
public sealed class DirectController : ControllerBase
{
    [HttpGet("ok")]
    public Result<Forecast> OkDirect() => Results.Ok(new Forecast { TemperatureC = 5 });

    [HttpGet("ok-explicit")]
    public IActionResult OkExplicit() => Results.Ok(new Forecast { TemperatureC = 5 }).ToActionResult();

    [HttpGet("fail")]
    public async Task<Result<Forecast>> Fail()
    {
        await Task.Yield();
        return new ProblemResult(404, "Missing");
    }

    [HttpGet("fail-explicit")]
    public IActionResult FailExplicit() => new Result<Forecast>(new ProblemResult(404, "Missing")).ToActionResult();

    [HttpGet("plain")]
    public IActionResult Plain() => Ok("x");

    [HttpGet("object")]
    public Forecast Object() => new() { TemperatureC = 9 };
}
