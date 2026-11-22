# Contract: Benchmark Catalog

| Class | Benchmark | Scenario (FR-001) |
|-------|-----------|-------------------|
| ResultBenchmarks | `Success` | success result without value |
| ResultBenchmarks | `SuccessWithValue` | success `Result<T>` with value |
| ResultBenchmarks | `Failure` | failure result with a problem |
| ResultBenchmarks | `FailureTyped` | failure `Result<T>` |
| ResultBenchmarks | `ImplicitFromValue` | value → `Result<T>` |
| ResultBenchmarks | `ImplicitFromFailure` | untyped failure → `Result<T>` |
| ProblemBenchmarks | `CtorWithTitle` | `new ProblemResult(404, "Custom")` |
| ProblemBenchmarks | `CtorWithoutTitle` | `new ProblemResult(404, "")` (reason-phrase default) |
| ProblemBenchmarks | `BuilderWithTitle` | builder + `WithTitle` |
| ProblemBenchmarks | `BuilderWithoutTitle` | builder, status only |
| JsonBenchmarks | `Serialize` | problem without extensions → JSON |
| JsonBenchmarks | `SerializeWithExtensions` | problem with 3 extensions → JSON |
| JsonBenchmarks | `Deserialize` | JSON without extensions → problem |
| JsonBenchmarks | `DeserializeWithExtensions` | JSON with 3 extensions → problem |

Run surface:
- `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*'`
- Filter by class: `--filter '*JsonBenchmarks*'`; by method: `--filter '*Deserialize*'`
