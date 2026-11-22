using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Viana.Results;

namespace Viana.Results.Benchmarks;

/// <summary>ProblemResultJsonConverter serialization and deserialization.</summary>
[MemoryDiagnoser]
public class JsonBenchmarks
{
    private JsonSerializerOptions _options = null!;
    private ProblemResult _problem = null!;
    private ProblemResult _problemWithExtensions = null!;
    private string _json = null!;
    private string _jsonWithExtensions = null!;

    [GlobalSetup]
    public void Setup()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new ProblemResultJsonConverter());

        _problem = new ProblemResult(404, "Not Found");
        _problemWithExtensions = new ProblemResult(404, "Not Found", "about:blank", new Dictionary<string, object?>
        {
            ["traceId"] = "00-abc-01",
            ["count"] = 3,
            ["retry"] = true,
        });

        _json = JsonSerializer.Serialize(_problem, _options);
        _jsonWithExtensions = JsonSerializer.Serialize(_problemWithExtensions, _options);
    }

    [Benchmark]
    public string Serialize() => JsonSerializer.Serialize(_problem, _options);

    [Benchmark]
    public string SerializeWithExtensions() => JsonSerializer.Serialize(_problemWithExtensions, _options);

    [Benchmark]
    public ProblemResult? Deserialize() => JsonSerializer.Deserialize<ProblemResult>(_json, _options);

    [Benchmark]
    public ProblemResult? DeserializeWithExtensions() => JsonSerializer.Deserialize<ProblemResult>(_jsonWithExtensions, _options);
}
