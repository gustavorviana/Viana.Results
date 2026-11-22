# Data Model

## Benchmark scenario
| Field | Rule |
|-------|------|
| Class | `ResultBenchmarks`, `ProblemBenchmarks` or `JsonBenchmarks` (filter group) |
| Method | Unique, descriptive name (filter target) |
| Metrics | Mean, Error, StdDev, Allocated (MemoryDiagnoser) |

## Baseline
| Field | Rule |
|-------|------|
| File | `benchmarks/Viana.Results.Benchmarks/baseline/baseline-<core version>.md` |
| Header | Core version, date, BenchmarkDotNet environment block (OS, CPU, runtime) |
| Body | One GitHub Markdown table per benchmark class |
