namespace Viana.Results.AspNetCore.Tests.TestHost;

public sealed class Forecast
{
    public int TemperatureC { get; set; }

    [Newtonsoft.Json.JsonProperty("renamed")]
    public string? Summary { get; set; }
}
