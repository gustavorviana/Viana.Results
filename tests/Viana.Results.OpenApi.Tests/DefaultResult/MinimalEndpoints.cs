using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>Minimal endpoints covering the feature 011 documentation rules.</summary>
public static class MinimalEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // (b) bare Result → 204, no inferred 200.
        app.MapGet("/min/bare", () => Results.Ok()).ProducesProblemResult(404, title: "Item not found");

        // (c) bare Result + Produces(204) → 204 once, no 200.
        app.MapPut("/min/bare-produces", () => Results.NoContent()).Produces(StatusCodes.Status204NoContent);

        // (d) bare Result + Produces(202) → 202 only.
        app.MapPost("/min/bare-202", () => new Result(202)).Produces(StatusCodes.Status202Accepted);

        // (e) Result<T> without declaration → 200 with T.
        app.MapGet("/min/typed", () => Results.Ok(new Item(1, "one")));

        // (g) ProducesResult of a provider returning Results.Ok() → 204.
        app.MapDelete("/min/empty-example", () => Results.Ok()).ProducesResult<EmptyOk>();

        // (g3) Result<T> + ProducesResult of a 202 provider → 202 only.
        app.MapPost("/min/typed-202", () => new Result<Ticket>(new Ticket("T-1"), 202)).ProducesResult<TicketAccepted>();

        // (h) WithoutDefaultResult with two hand-registered successes → exactly 200 and 202.
        app.MapGet("/min/multi", IResult (bool later) => later ? new Result<Ticket>(new Ticket("T-1"), 202) : Results.Ok(new Item(1, "one")))
            .WithoutDefaultResult()
            .ProducesResult<ItemOk>()
            .ProducesResult<TicketAccepted>()
            .ProducesProblemResult<ItemNotFound>();

        // (k) WithoutDefaultResult with nothing declared → no success, problems kept.
        app.MapGet("/min/none", () => Results.Ok(new Item(1, "one")))
            .WithoutDefaultResult()
            .ProducesProblemResult(404, title: "Item not found");

        // (l) WithoutDefaultResult on a group applies to every endpoint in it.
        var group = app.MapGroup("/min/group").WithoutDefaultResult();
        group.MapGet("/bare", () => Results.Ok());
        group.MapGet("/typed", () => Results.Ok(new Item(1, "one")));
    }
}
