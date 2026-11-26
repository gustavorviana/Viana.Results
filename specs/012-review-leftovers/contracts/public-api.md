# Public API changes

## Viana.Results.Mediators
```csharp
public static class MediatorDiagnostics
{
    /// Recommended seconds-based bucket boundaries for DurationInstrument (advised on .NET 9+).
    public static IReadOnlyList<double> DurationBucketBoundaries { get; }
    // [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
}
```

## Viana.Results.OpenApi (behavior)
- `ProducesProblemResult<P>()`, `ProducesProblemResult(typeof(P))` and `ProducesResult<P>()` accept an
  `IExampleProvider<ProblemResult>` and document it as a problem with its own status. Inline overloads are
  unchanged (`IResult`); wrap a `ProblemResult` with `new Result(problem)`.
- A problem declaration whose example is not a Viana result and not a `ProblemResult` fails document generation
  with a message naming the endpoint and the provider.
