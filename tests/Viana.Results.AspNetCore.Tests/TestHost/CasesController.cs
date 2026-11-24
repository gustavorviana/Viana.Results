using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.AspNetCore.Tests.TestHost;

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
