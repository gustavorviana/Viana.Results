namespace Viana.Results.Examples.Shared;

/// <summary>
/// Daily weather forecast for a given date.
/// </summary>
/// <remarks>
/// TemperatureF is computed from TemperatureC and is therefore read-only —
/// clients should not send it on POST/PUT requests.
/// </remarks>
public class WeatherForecast
{
    /// <summary>The calendar date the forecast applies to.</summary>
    /// <example>2026-05-15</example>
    public DateOnly Date { get; set; }

    /// <summary>Temperature in degrees Celsius. Realistic range: -40 to 60.</summary>
    /// <example>22</example>
    public int TemperatureC { get; set; }

    /// <summary>Temperature in degrees Fahrenheit, derived from TemperatureC.</summary>
    /// <example>71</example>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>Short qualitative description of the weather (e.g. "Mild", "Hot").</summary>
    /// <example>Mild</example>
    public string? Summary { get; set; }
}
