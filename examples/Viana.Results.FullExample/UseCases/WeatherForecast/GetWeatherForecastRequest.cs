using Viana.Results.Mediators;

namespace Viana.Results.FullExample.UseCases.WeatherForecast;

using WeatherForecastModel = FullExample.WeatherForecast;

public record GetWeatherForecastRequest(int? Count = null) : IRequest<ListResult<WeatherForecastModel>>;
