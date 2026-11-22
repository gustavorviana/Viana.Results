using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>Example returning the real Result (status 200 and body come from it).</summary>
public sealed class ForecastResultExample : IExampleProvider<Result<WeatherForecast>>
{
    public Result<WeatherForecast> GetExample() => Results.Ok(new WeatherForecast
    {
        Date = new DateOnly(2026, 5, 15),
        TemperatureC = 22,
        Summary = "Mild"
    });
}

/// <summary>Failed Result example (status 404 comes from it).</summary>
public sealed class ForecastNotFoundExample : IExampleProvider<Result>
{
    public Result GetExample() => new ProblemBuilder(404)
        .WithTitle("Forecast not found")
        .WithDetail("There is no forecast for the requested day.")
        .AddExtension("day", 3)
        .BuildResult();
}
