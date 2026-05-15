using System.ComponentModel.DataAnnotations;

namespace Viana.Results.Examples.Shared.Requests;

/// <summary>
/// Payload to register a new weather forecast.
/// </summary>
/// <remarks>
/// Used to demonstrate how XML documentation on a request body shows up in the generated
/// OpenAPI schema (description + example), both in Swagger and Scalar.
/// </remarks>
public class CreateWeatherForecastRequest
{
    /// <summary>The calendar date the forecast applies to. Cannot be in the past.</summary>
    /// <example>2026-05-20</example>
    [Required]
    public DateOnly Date { get; set; }

    /// <summary>Temperature in degrees Celsius. Accepted range: -40 to 60.</summary>
    /// <example>23</example>
    [Range(-40, 60)]
    public int TemperatureC { get; set; }

    /// <summary>Short qualitative description of the weather (e.g. "Mild", "Hot"). Max 64 characters.</summary>
    /// <example>Warm</example>
    [MaxLength(64)]
    public string? Summary { get; set; }
}
