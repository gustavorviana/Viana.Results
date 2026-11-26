using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>(j) WithoutDefaultResult on the class applies to every action.</summary>
[ApiController]
[Route("marked")]
[WithoutDefaultResult]
public sealed class MarkedController : ControllerBase
{
    /// <summary>Bare result: no inferred 204.</summary>
    [HttpGet("bare")]
    public Result Bare() => Results.Ok();

    /// <summary>Typed result: no inferred 200.</summary>
    [HttpGet("typed")]
    public Result<Item> Typed() => new Item(1, "one");
}
