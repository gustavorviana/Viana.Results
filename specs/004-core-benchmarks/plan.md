# Implementation Plan: Core Benchmarks

**Branch**: `chore/add-spec-kit` (shared) | **Date**: 2026-11-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/004-core-benchmarks/spec.md`

## Summary

Add `benchmarks/Viana.Results.Benchmarks`, a BenchmarkDotNet console app (net10.0) referencing the
core project, with three benchmark classes (results, problems, JSON) using `[MemoryDiagnoser]`. It is
added to `Viana.Results.slnx` so it builds in CI under the same rules, but it is not a test project
and is never executed by `dotnet test`. A README documents running, filtering and comparing, and a
Markdown baseline is committed.

## Technical Context

**Language/Version**: C# (latest), .NET SDK 10, target `net10.0`

**Primary Dependencies**: BenchmarkDotNet 0.15.8 (central version in `Directory.Packages.props`)

**Storage**: Baseline report as Markdown in `benchmarks/Viana.Results.Benchmarks/baseline/`

**Testing**: Build-only in CI; manual run in Release (`--filter` support from BenchmarkDotNet)

**Target Platform**: developer machine (Windows/Linux/macOS)

**Project Type**: console app (benchmarks), not packable

**Performance Goals**: N/A (this feature creates the measurement)

**Constraints**: TreatWarningsAsErrors, NuGet audit `all` (BenchmarkDotNet transitive graph must be
clean), `IsPackable=false`, no core code changes

**Scale/Scope**: 1 project, 3 benchmark classes, ~14 benchmarks, README, baseline

## Constitution Check

| Principle | Status | Note |
|-----------|--------|------|
| I. Minimal core | PASS | Dependency only in the benchmarks project; core untouched |
| III. TDD | PASS (N/A) | No production behavior; benchmarks are measurement tooling |
| V. Performance & benchmarks | PASS | This feature implements the benchmarks the principle requires |
| VI. AOT | PASS | Core unchanged |
| VIII. Docs | PASS | `benchmarks/README.md` + baseline |
| Technical constraints | PASS | Inherits root `Directory.Build.props` |

Post-design re-check: PASS.

## Project Structure

### Documentation (this feature)

```text
specs/004-core-benchmarks/
├── plan.md, research.md, data-model.md, quickstart.md
├── contracts/benchmark-catalog.md
└── tasks.md (next)
```

### Source Code

```text
benchmarks/
├── README.md
└── Viana.Results.Benchmarks/
    ├── Viana.Results.Benchmarks.csproj
    ├── Program.cs                 # BenchmarkSwitcher.FromAssembly(...).Run(args)
    ├── ResultBenchmarks.cs
    ├── ProblemBenchmarks.cs
    ├── JsonBenchmarks.cs
    └── baseline/
        └── baseline-2.1.0.md      # merged GitHub-flavored reports + environment
Directory.Packages.props           # + BenchmarkDotNet
Viana.Results.slnx                 # + benchmarks project (folder /benchmarks/)
```

**Structure Decision**: Separate top-level `benchmarks/` folder, mirroring `examples/` and `tests/`.

## Complexity Tracking

None.
