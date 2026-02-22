using Viana.Results.Mediators;

namespace Viana.Results.FullExample.UseCases.WeatherForecast;

// Model from root namespace to avoid conflict with current namespace name
using WeatherForecastModel = FullExample.WeatherForecast;

public class GetWeatherForecastHandler : IHandler<GetWeatherForecastRequest, ListResult<WeatherForecastModel>>
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public Task<ListResult<WeatherForecastModel>> Handle(GetWeatherForecastRequest request, CancellationToken cancellationToken = default)
    {
        var count = request.Count is > 0 and <= 20 ? request.Count.Value : 5;
        var forecasts = Enumerable.Range(1, count).Select(index => new WeatherForecastModel
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        return Task.FromResult(new ListResult<WeatherForecastModel>(forecasts));
    }
}