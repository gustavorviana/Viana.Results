using Microsoft.AspNetCore.Mvc;
using Viana.Results.FullExample.UseCases.WeatherForecast;
using Viana.Results.Mediators;

namespace Viana.Results.FullExample.Controllers
{
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

        /// <summary>Retorna previsões usando Result diretamente (sem Mediator).</summary>
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

        /// <summary>Retorna previsões via Mediator (request/handler).</summary>
        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ListResult<WeatherForecast>> Get([FromQuery] int? count = null, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.SendAsync(new GetWeatherForecastRequest(count), cancellationToken);
            return result;
        }
    }
}
