using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.AspNetCore.Tests.TestHost;

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

    [HttpGet("ok-empty")]
    public IActionResult OkEmpty() => Results.Ok().ToActionResult();

    [HttpGet("explicit-200")]
    public IActionResult Explicit200() => new Result(200).ToActionResult();

    [HttpGet("null-value")]
    public IActionResult NullValue() => new Result<Forecast>((Forecast?)null).ToActionResult();

    [HttpGet("list")]
    public IActionResult List() => new ListResult<Forecast>(new List<Forecast> { new() { TemperatureC = 1 }, new() { TemperatureC = 2 } }).ToActionResult();

    [HttpGet("paged")]
    public IActionResult Paged() => new PagedResult<Forecast>(new List<Forecast> { new() { TemperatureC = 1 } }, 1, 3).ToActionResult();
}
