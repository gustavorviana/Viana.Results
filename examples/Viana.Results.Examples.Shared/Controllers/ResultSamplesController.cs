using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Controllers;

/// <summary>
/// One route per core factory and result type, so every way of building a result can be called and
/// compared with its documentation.
/// </summary>
[ApiController]
[Route("api/samples")]
public class ResultSamplesController : ControllerBase
{
    private static readonly Product Sample = new(1, "KB-001", "Mechanical keyboard", 349.90m, 10);

    // ---------- Successes ----------

    /// <summary><c>Results.Ok()</c>: success without a value (bare <c>Result</c>, no body).</summary>
    [HttpGet("ok")]
    public Result OkEmpty() => Results.Ok();

    /// <summary><c>Results.Ok(string)</c>: a message as the body.</summary>
    [HttpGet("ok-message")]
    public Result<string> OkMessage() => Results.Ok("Everything is fine.");

    /// <summary><c>Results.Ok(value)</c>: 200 with the value.</summary>
    [HttpGet("ok-value")]
    public Result<Product> OkValue() => Results.Ok(Sample);

    /// <summary>Implicit conversion: returning the value itself produces a 200 <c>Result&lt;T&gt;</c>.</summary>
    [HttpGet("implicit")]
    public Result<Product> Implicit() => Sample;

    /// <summary><c>Results.Created(value)</c>: 201 with the value.</summary>
    [HttpPost("created")]
    [ResponseExample(typeof(ProductCreatedExample))]
    public Result<Product> CreatedSample() => Results.Created(Sample);

    /// <summary><c>Results.NoContent()</c>: 204 without body.</summary>
    [HttpDelete("no-content")]
    public Result NoContentSample() => Results.NoContent();

    /// <summary><c>ListResult&lt;T&gt;</c>: a list, written as a JSON array (implicit from <c>List&lt;T&gt;</c>).</summary>
    [HttpGet("list")]
    public ListResult<Product> List() => new List<Product> { Sample, Sample with { Id = 2, Sku = "MS-001", Name = "Wireless mouse" } };

    /// <summary><c>PagedResult&lt;T&gt;.Create</c>: a page with paging metadata.</summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Items per page.</param>
    [HttpGet("paged")]
    public PagedResult<int> Paged([FromQuery] int page = 1, [FromQuery] int pageSize = 3)
    {
        var all = Enumerable.Range(1, 10).ToList();
        var number = Math.Max(1, page);
        return PagedResult<int>.Create(all.Skip((number - 1) * pageSize).Take(pageSize).ToList(), number, pageSize, all.Count);
    }

    /// <summary>
    /// Last resort: the action returns the common <c>IResult</c> but answers two different successes (200 with the
    /// product, or 202 with a reservation). <c>[WithoutDefaultResult]</c> drops the inferred success, and the
    /// successes are documented by hand. Declare <c>IResult</c>, not <c>Result</c>: a <c>Result&lt;T&gt;</c>
    /// converted to <c>Result</c> keeps only the status, not the body.
    /// </summary>
    /// <param name="reserve">true answers 202 with a reservation; false answers 200 with the product.</param>
    [HttpGet("multi-success")]
    [WithoutDefaultResult]
    [ProducesResponseType<Product>(StatusCodes.Status200OK)]
    [ProducesResponseType<Reservation>(StatusCodes.Status202Accepted)]
    [ResponseExample(typeof(ProductExample))]
    [ResponseExample(typeof(ReservationAcceptedExample))]
    public IResult MultiSuccess([FromQuery] bool reserve = false)
        => reserve ? new Result<Reservation>(new Reservation(Sample.Id, "R-0001"), 202) : Results.Ok(Sample);

    // ---------- Problems (RFC 9457) ----------

    /// <summary><c>Results.BadRequest(title)</c>: 400.</summary>
    [HttpGet("bad-request")]
    [ProblemResult(400, Title = "The request is malformed.")]
    public Result BadRequestSample() => Results.BadRequest("The request is malformed.");

    /// <summary><c>Results.Validation(errors)</c>: validation problem with an <c>errors</c> member.</summary>
    [HttpGet("validation")]
    [ResponseExample(typeof(InvalidProductExample))]
    public Result Validation() => Results.Validation(new Dictionary<string, string[]>
    {
        ["sku"] = ["The SKU is required."],
        ["price"] = ["The price must be greater than zero."]
    });

    /// <summary><c>Results.Unauthorized()</c>: 401.</summary>
    [HttpGet("unauthorized")]
    [ProblemResult(401)]
    public Result UnauthorizedSample() => Results.Unauthorized();

    /// <summary><c>Results.Forbidden(title)</c>: 403.</summary>
    [HttpGet("forbidden")]
    [ProblemResult(403, Title = "Only administrators can do this.")]
    public Result ForbiddenSample() => Results.Forbidden("Only administrators can do this.");

    /// <summary><c>Results.NotFound(title)</c>: 404.</summary>
    [HttpGet("not-found")]
    [ResponseExample(typeof(ProductNotFoundExample))]
    public Result NotFoundSample() => Results.NotFound("Product 42 not found.");

    /// <summary><c>Results.Conflict(title)</c>: 409.</summary>
    [HttpGet("conflict")]
    [ResponseExample(typeof(DuplicateSkuExample))]
    public Result ConflictSample() => Results.Conflict("SKU 'KB-001' is already in use.");

    /// <summary><c>Results.BusinessRuleViolated(message)</c>: 422.</summary>
    [HttpGet("business-rule")]
    [ResponseExample(typeof(ProductHasStockExample))]
    public Result BusinessRule() => Results.BusinessRuleViolated("Product 1 still has 10 units in stock.");

    /// <summary><c>Results.Failure(status, title)</c>: any status, here 503.</summary>
    [HttpGet("failure")]
    [ResponseExample(typeof(ServiceUnavailableExample))]
    public Result Failure() => Results.Failure(HttpStatusCode.ServiceUnavailable, "Try again later.");

    /// <summary><c>Results.Failure(exception)</c>: an exception you caught, as a 500 problem.</summary>
    [HttpGet("failure-exception")]
    [ProblemResult(500, Title = "Internal Server Error")]
    public Result FailureFromException()
    {
        try
        {
            throw new InvalidOperationException("The price table is corrupted.");
        }
        catch (InvalidOperationException ex)
        {
            return Results.Failure(ex);
        }
    }

    /// <summary><c>ProblemBuilder</c>: title, type, detail, instance and extensions, fluent.</summary>
    [HttpGet("builder")]
    [ProblemResult(409, Title = "Reservation expired", Type = "https://api.example.com/errors/reservation-expired", Detail = "The reservation 77 expired at 10:00.")]
    public Result Builder() => new ProblemBuilder(409)
        .WithTitle("Reservation expired")
        .WithType("https://api.example.com/errors/reservation-expired")
        .WithDetail("The reservation 77 expired at 10:00.")
        .WithInstance("/api/reservations/77")
        .AddExtension("reservationId", 77)
        .AddExtensions([new("expiredAt", "2026-11-23T10:00:00Z")])
        .BuildResult();

    /// <summary><c>ProblemResult.WithDetail</c> converted implicitly to <c>Result&lt;T&gt;</c>.</summary>
    [HttpGet("problem-result")]
    [ProblemResult(402, Title = "Payment Required", Detail = "The account has no credit.")]
    public Result<Product> ProblemResultSample()
        => ProblemResult.WithDetail(402, "Payment Required", "The account has no credit.");

    /// <summary>An unhandled exception: the host's exception handler answers a 500 problem.</summary>
    [HttpGet("crash")]
    [ProblemResult(500, Title = "Internal Server Error")]
    public Result Crash() => throw new InvalidOperationException("Unhandled on purpose.");
}
