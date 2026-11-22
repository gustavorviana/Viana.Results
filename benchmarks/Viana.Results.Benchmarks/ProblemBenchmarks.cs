using BenchmarkDotNet.Attributes;
using Viana.Results;

namespace Viana.Results.Benchmarks;

/// <summary>ProblemResult creation, directly and through the builder, with and without a title.</summary>
[MemoryDiagnoser]
public class ProblemBenchmarks
{
    [Benchmark]
    public ProblemResult CtorWithTitle() => new(404, "Custom");

    [Benchmark]
    public ProblemResult CtorWithoutTitle() => new(404, "");

    [Benchmark]
    public ProblemResult BuilderWithTitle() => new ProblemBuilder(404).WithTitle("Custom").Build();

    [Benchmark]
    public ProblemResult BuilderWithoutTitle() => new ProblemBuilder(404).Build();
}
