using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Viana.Results.AspNetCore;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared;

/// <summary>Minimal API sample: handlers return Viana results directly; responses documented with Viana helpers.</summary>
public static class MinimalForecastEndpoints
{
    public static void MapMinimalForecasts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/minimal/forecasts").AddVianaResults();

        group.MapGet("/{day:int}", (int day) => day is >= 0 and < 7
                ? (IResult)Results.Ok(new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Today.AddDays(day)), TemperatureC = 20 + day, Summary = "Mild" })
                : new ProblemBuilder(404).WithTitle("Forecast not found").WithDetail("There is no forecast for the requested day.").AddExtension("day", day).BuildResult())
            .WithName("GetMinimalForecast")
            .ProducesResult<ForecastResultExample>()
            .ProducesProblemResult<ForecastNotFoundExample>();
    }
}
