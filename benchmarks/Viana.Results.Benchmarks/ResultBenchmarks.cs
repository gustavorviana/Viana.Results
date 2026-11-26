using BenchmarkDotNet.Attributes;
using Viana.Results;

namespace Viana.Results.Benchmarks;

/// <summary>Result creation and implicit conversions.</summary>
[MemoryDiagnoser]
public class ResultBenchmarks
{
    private ProblemResult _problem = null!;

    [GlobalSetup]
    public void Setup() => _problem = new ProblemResult(404, "Not Found");

    [Benchmark]
    public Result Success() => Results.Ok();

    [Benchmark]
    public Result<int> SuccessWithValue() => Results.Ok(42);

    [Benchmark]
    public Result Failure() => Results.Failure(404);

    [Benchmark]
    public Result<int> FailureTyped()
    {
        Result<int> result = Results.Failure(404);
        return result;
    }

    [Benchmark]
    public Result<string> ImplicitFromValue()
    {
        Result<string> result = "value";
        return result;
    }

    [Benchmark]
    public Result<string> ImplicitFromFailure()
    {
        Result<string> result = _problem;
        return result;
    }
}
