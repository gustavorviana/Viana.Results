using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

public sealed class WeatherForecastSuccessExample : IExampleProvider<WeatherForecast>
{
    public WeatherForecast GetExample() => new()
    {
        Date = new DateOnly(2026, 5, 15),
        TemperatureC = 22,
        Summary = "Mild"
    };
}
