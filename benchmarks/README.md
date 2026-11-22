# Benchmarks

Performance measurements for the hot paths of the core package (`Viana.Results`), as required by
constitution Principle V: changes to hot paths must include before/after BenchmarkDotNet results in
the PR, and any time or allocation regression must be justified.

## Prerequisites

- .NET SDK 10.
- Always run in **Release** (`-c Release`). BenchmarkDotNet refuses non-optimized builds.
- Close heavy applications; keep the machine on AC power.

## Run

```bash
# Everything (takes several minutes)
dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*'

# One class
dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*JsonBenchmarks*'

# Methods by name
dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*Deserialize*'

# Quick check (less precise, much faster)
dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*ProblemBenchmarks*' --job short
```

Reports are written to `BenchmarkDotNet.Artifacts/results/` (git-ignored).

## Catalog

| Class | What it measures |
|-------|------------------|
| [`ResultBenchmarks`](Viana.Results.Benchmarks/ResultBenchmarks.cs) | `Result`/`Result<T>` success and failure, implicit conversions |
| [`ProblemBenchmarks`](Viana.Results.Benchmarks/ProblemBenchmarks.cs) | `ProblemResult` and `ProblemBuilder`, with and without title (reason-phrase default) |
| [`JsonBenchmarks`](Viana.Results.Benchmarks/JsonBenchmarks.cs) | `ProblemResultJsonConverter` write/read, with and without extensions |

## Compare before and after a change

1. On `main` (or before your change), run the affected classes with
   `--exporters github` and keep the `*-report-github.md` files.
2. Apply your change and run the same command **on the same machine**.
3. Compare `Mean` and `Allocated` per method, side by side, and paste both tables in the PR.
4. Read the results this way:
   - **Allocated** is deterministic: any increase is a regression and must be justified.
   - **Mean** varies between runs; treat differences within the reported `Error` as noise. A
     difference clearly outside it is a regression (or improvement) to explain in the PR.
   - Absolute times are only comparable on the same machine. The committed baseline
     ([`baseline/`](Viana.Results.Benchmarks/baseline/)) shows orders of magnitude and allocations,
     not a target for your machine.

## Refresh the baseline

When a change intentionally alters performance, or a new core version is released, run the full suite
with `--exporters github` and add `baseline/baseline-<core version>.md` with the version, date, commit
and environment block. Keep older baselines for history.

## CI

CI builds this project (so it follows the zero-warnings rule) but never runs the benchmarks: shared
runners are too noisy for meaningful numbers.
