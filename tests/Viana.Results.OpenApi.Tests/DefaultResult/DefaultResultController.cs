using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>Controller actions covering the feature 011 documentation rules.</summary>
[ApiController]
[Route("ctl")]
public sealed class DefaultResultController : ControllerBase
{
    /// <summary>(a) bare Result, nothing declared → 204.</summary>
    [HttpGet("bare")]
    [ProblemResult(404, Title = "Item not found")]
    public Result Bare() => Results.Ok();

    /// <summary>(d) bare Result with a hand-declared 202 → 202 only.</summary>
    [HttpGet("bare-202")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public Result Bare202() => new Result(202);

    /// <summary>(e) Result&lt;T&gt; without declaration → 200 with T.</summary>
    [HttpGet("typed")]
    public Result<Item> Typed() => new Item(1, "one");

    /// <summary>(g2) Result&lt;T&gt; whose example result is Created → 201 only.</summary>
    [HttpPost("created")]
    [ResponseExample(typeof(ItemCreated))]
    public Result<Item> CreateItem() => Results.Created(new Item(2, "two"));

    /// <summary>(i) WithoutDefaultResult with two hand-registered successes → exactly those.</summary>
    [HttpGet("multi")]
    [WithoutDefaultResult]
    [ProducesResponseType<Item>(StatusCodes.Status200OK)]
    [ProducesResponseType<Ticket>(StatusCodes.Status202Accepted)]
    [ResponseExample(typeof(ItemOk))]
    [ResponseExample(typeof(TicketAccepted))]
    public IResult Multi([FromQuery] bool later) => later ? new Result<Ticket>(new Ticket("T-1"), 202) : Results.Ok(new Item(1, "one"));

    /// <summary>(k) WithoutDefaultResult with nothing declared → no success, problems kept.</summary>
    [HttpGet("none")]
    [WithoutDefaultResult]
    [ProblemResult(404, Title = "Item not found")]
    public Result<Item> None() => new Item(1, "one");
}
