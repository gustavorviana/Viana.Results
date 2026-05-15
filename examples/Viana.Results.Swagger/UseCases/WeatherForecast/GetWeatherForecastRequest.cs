using Viana.Results.Mediators;

namespace Viana.Results.Swagger.UseCases.WeatherForecast;

using WeatherForecastModel = Swagger.WeatherForecast;

public record GetWeatherForecastRequest(int? Count = null) : IRequest<ListResult<WeatherForecastModel>>;
