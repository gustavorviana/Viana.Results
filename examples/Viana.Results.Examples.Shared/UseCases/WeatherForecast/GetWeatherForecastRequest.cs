using Viana.Results.Mediators;
using WeatherForecastModel = Viana.Results.Examples.Shared.WeatherForecast;

namespace Viana.Results.Examples.Shared.UseCases.WeatherForecast;

/// <summary>
/// Request to fetch a list of weather forecasts starting tomorrow.
/// </summary>
/// <param name="Count">
/// How many days to return. Optional; when omitted (or out of the accepted range 1-20)
/// the handler falls back to 5.
/// </param>
/// <example>{ "count": 7 }</example>
public record GetWeatherForecastRequest(int? Count = null) : IRequest<ListResult<WeatherForecastModel>>;
