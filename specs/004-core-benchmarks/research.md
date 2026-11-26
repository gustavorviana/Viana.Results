# Research: Core Benchmarks

## R1 – Tool
- **Decision**: BenchmarkDotNet 0.15.8 with `[MemoryDiagnoser]`.
- **Rationale**: De-facto standard for .NET; reports mean, error, allocations; supports `--filter`,
  Markdown (GitHub) exporters and refuses non-optimized builds.
- **Alternatives**: Stopwatch loops (no warmup/statistics), NBench (unmaintained).

## R2 – Include in the solution?
- **Decision**: Yes, add to `Viana.Results.slnx`.
- **Rationale**: CI builds it (FR-005 enforced, no bit-rot). `dotnet test` on the solution only runs
  test projects (no `Microsoft.NET.Test.Sdk` here), so benchmarks are never executed (FR-004).
- **Alternatives**: Keep outside solution (would not be built by CI; warnings could accumulate).

## R3 – Packaging
- **Decision**: `IsPackable=false`, `OutputType=Exe`.

## R4 – Audit risk
- **Decision**: Restore with audit mode `all`; if a transitive advisory appears, pin the fixed version
  centrally (feature 001 policy), never suppress.

## R5 – Baseline format
- **Decision**: Run with `--exporters github`, combine the per-class `*-report-github.md` files plus the
  environment header into `baseline/baseline-<core version>.md`. Raw `BenchmarkDotNet.Artifacts/` is
  git-ignored.
- **Rationale**: Readable in review; versioned name allows multiple baselines over time.

## R6 – Comparison
- **Decision**: Document a manual side-by-side comparison of the new report vs the baseline on the same
  machine; allocations are the stable signal. No extra tooling for now.
- **Alternatives**: `ResultsComparer` tool from dotnet/performance (extra setup; can be added later).

## R7 – Scenario data
- **Decision**: Fixed inputs created in `[GlobalSetup]` (pre-built JSON strings, extension dictionary of
  3 entries, `JsonSerializerOptions` instance with the converter) so only the operation is measured.
