using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.AspNetCore.Tests.TestHost;

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
