using Microsoft.AspNetCore.Mvc;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Controllers;

/// <summary>
/// Sample controller exposing one endpoint per supported result type:
/// Result, Result of T, ListResult of T and PagedResult of T.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ResultSamplesController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>Returns a bodyless Result (HTTP 204 No Content).</summary>
    [HttpPost("result", Name = "GetResultOnly")]
    public Result GetResultOnly()
    {
        return Results.NoContent();
    }

    /// <summary>
    /// Returns a Result of T with a single item. Documents 500 and 503 error responses
    /// via ProblemResult attributes; the globally-registered 500 example also applies here.
    /// </summary>
    [HttpGet("result-typed", Name = "GetResultTyped")]
    [ProblemResult(500, Title = "Internal Server Error")]
    [ProblemResult(503, Title = "Service Unavailable")]
    [ResponseExample(200, typeof(WeatherForecastSuccessExample), Summary = "Typical forecast", Description = "A successful response for the typed endpoint.")]
    [ResponseExample(503, typeof(ServiceUnavailableExample), Summary = "Downstream provider down")]
    public Result<WeatherForecast> GetResultTyped()
    {
        var item = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        };
        return Results.Ok(item);
    }

    /// <summary>Returns a ListResult of T — a list of items.</summary>
    [HttpGet("list", Name = "GetListResult")]
    public ListResult<WeatherForecast> GetListResult()
    {
        var items = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToList();
        return items;
    }

    /// <summary>Returns a PagedResult of T — a page of items with paging metadata.</summary>
    [HttpGet("paged", Name = "GetPagedResult")]
    public PagedResult<WeatherForecast> GetPagedResult([FromQuery] int page = 1, [FromQuery] int pageSize = 3)
    {
        var all = Enumerable.Range(1, 10).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToList();
        var totalCount = all.Count;
        var pageNumber = Math.Max(1, page);
        var skip = (pageNumber - 1) * pageSize;
        var data = all.Skip(skip).Take(pageSize).ToList();
        return PagedResult<WeatherForecast>.Create(data, pageNumber, pageSize, totalCount);
    }
}
