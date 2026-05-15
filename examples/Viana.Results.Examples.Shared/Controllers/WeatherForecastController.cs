using Microsoft.AspNetCore.Mvc;
using Viana.Results.Mediators;
using Viana.Results.Examples.Shared.Requests;
using Viana.Results.Examples.Shared.UseCases.WeatherForecast;

namespace Viana.Results.Examples.Shared.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IMediator _mediator;
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public WeatherForecastController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns 5 forecasts starting tomorrow, bypassing the mediator pipeline.
    /// </summary>
    /// <returns>An array of forecasts for the next 5 days.</returns>
    [HttpGet("direct", Name = "GetWeatherForecastDirect")]
    public Result<WeatherForecast[]> GetDirect()
    {
        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        return Results.Ok(forecasts);
    }

    /// <summary>
    /// Returns weather forecasts through the mediator pipeline, demonstrating the
    /// IHandler flow (request and handler).
    /// </summary>
    /// <param name="count">How many days to return (1-20). Defaults to 5 when omitted or out of range.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A list of forecasts for the next count days.</returns>
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ListResult<WeatherForecast>> Get([FromQuery] int? count = null, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.SendAsync(new GetWeatherForecastRequest(count), cancellationToken);
        return result;
    }

    /// <summary>
    /// Registers a new weather forecast. Demonstrates how the documentation for a request
    /// body (CreateWeatherForecastRequest) appears in the generated OpenAPI document.
    /// </summary>
    /// <param name="request">The forecast payload to register.</param>
    /// <returns>The registered forecast echoed back to the caller.</returns>
    [HttpPost(Name = "CreateWeatherForecast")]
    public Result<WeatherForecast> Create([FromBody] CreateWeatherForecastRequest request)
    {
        var created = new WeatherForecast
        {
            Date = request.Date,
            TemperatureC = request.TemperatureC,
            Summary = request.Summary
        };
        return Results.Created(created);
    }
}
