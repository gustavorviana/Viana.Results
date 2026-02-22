using Microsoft.AspNetCore.Mvc;
using Viana.Results;

namespace Viana.Results.FullExample.Controllers;

/// <summary>
/// Controller de exemplo com um endpoint para cada tipo de retorno: Result, Result&lt;T&gt;, ListResult&lt;T&gt;, PagedResult&lt;T&gt;.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ResultSamplesController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>Retorna apenas Result (sem body).</summary>
    [HttpGet("result", Name = "GetResultOnly")]
    public Result GetResultOnly()
    {
        return Results.NoContent();
    }

    /// <summary>Retorna Result&lt;T&gt; (um item).</summary>
    [HttpGet("result-typed", Name = "GetResultTyped")]
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

    /// <summary>Retorna ListResult&lt;T&gt; (lista de itens).</summary>
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

    /// <summary>Retorna PagedResult&lt;T&gt; (página de itens).</summary>
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
