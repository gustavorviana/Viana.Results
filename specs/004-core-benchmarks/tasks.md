# Tasks: Core Benchmarks

**Input**: Design documents from `specs/004-core-benchmarks/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/benchmark-catalog.md

**Tests**: No unit tests (measurement tooling, no production behavior). Validation is build + run per quickstart.md.

## Phase 1: Setup

- [X] T001 Add `<PackageVersion Include="BenchmarkDotNet" Version="0.15.8" />` to Directory.Packages.props
- [X] T002 Create benchmarks/Viana.Results.Benchmarks/Viana.Results.Benchmarks.csproj: `Microsoft.NET.Sdk`, `OutputType=Exe`, `TargetFramework=net10.0`, `IsPackable=false`, `ImplicitUsings=enable`, `<Optimize>true</Optimize>` for Release, `PackageReference` BenchmarkDotNet, `ProjectReference` ../../src/Viana.Results/Viana.Results.csproj; do NOT override `TreatWarningsAsErrors`/`Nullable` (inherited from root Directory.Build.props)
- [X] T003 Add the project to Viana.Results.slnx under a new `<Folder Name="/benchmarks/">`
- [X] T004 Append `BenchmarkDotNet.Artifacts/` to .gitignore (root)

## Phase 2: Foundational

- [X] T005 Create benchmarks/Viana.Results.Benchmarks/Program.cs: `BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);` (enables `--filter`, `--job`, `--exporters`)
- [X] T006 Run `dotnet restore Viana.Results.slnx` and `dotnet build Viana.Results.slnx -c Release`; must be 0 warnings including NuGet audit (`all`); if an advisory appears in BenchmarkDotNet's transitive graph, pin the fixed version in Directory.Packages.props (never suppress)

**Checkpoint**: Empty benchmarks app builds cleanly in the solution.

## Phase 3: User Story 1 – Measure core hot paths on demand (P1) 🎯 MVP

**Goal**: All 14 benchmarks of contracts/benchmark-catalog.md run with time and allocations.

**Independent Test**: `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*ProblemBenchmarks*' --job short` prints Mean and Allocated for only the 4 problem benchmarks.

- [X] T007 [P] [US1] Create benchmarks/Viana.Results.Benchmarks/ResultBenchmarks.cs (`[MemoryDiagnoser]`, `public class ResultBenchmarks`), each method returning its result: `Success` → `Results.Ok()`; `SuccessWithValue` → `Results.Ok(42)` (`Results.Ok<TValue>`; the typed value constructor is internal); `Failure` → `Results.Failure(404)`; `FailureTyped` → `Result<int>` from `Results.Failure(404)` via implicit conversion assigned to a typed local; `ImplicitFromValue` → `Result<string> r = "value"`; `ImplicitFromFailure` → `Result<string> r = (ProblemResult)` prebuilt problem from `[GlobalSetup]`
- [X] T008 [P] [US1] Create benchmarks/Viana.Results.Benchmarks/ProblemBenchmarks.cs (`[MemoryDiagnoser]`): `CtorWithTitle` → `new ProblemResult(404, "Custom")`; `CtorWithoutTitle` → `new ProblemResult(404, "")`; `BuilderWithTitle` → `new ProblemBuilder(404).WithTitle("Custom").Build()`; `BuilderWithoutTitle` → `new ProblemBuilder(404).Build()`
- [X] T009 [P] [US1] Create benchmarks/Viana.Results.Benchmarks/JsonBenchmarks.cs (`[MemoryDiagnoser]`): `[GlobalSetup]` builds a `JsonSerializerOptions` with `ProblemResultJsonConverter`, a problem without extensions, a problem with 3 extensions (`traceId` string, `count` int, `retry` bool), and their JSON strings; methods `Serialize`, `SerializeWithExtensions` (`JsonSerializer.Serialize(problem, options)`), `Deserialize`, `DeserializeWithExtensions` (`JsonSerializer.Deserialize<ProblemResult>(json, options)`)
- [X] T010 [US1] Build in Release (0 warnings) and run the independent test above; also run `--filter '*Deserialize*' --job short` and confirm only the 2 deserialize benchmarks run

**Checkpoint**: US1 complete.

## Phase 4: User Story 2 – Compare before/after (P2)

**Goal**: Baseline recorded and comparison documented.

**Independent Test**: Following benchmarks/README.md alone, a reader runs one class and compares it against the baseline file.

- [X] T011 [US2] Run the full suite in Release with default job: `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*' --exporters github`; create benchmarks/Viana.Results.Benchmarks/baseline/baseline-2.1.0.md with header (core version 2.1.0, date, commit hash) + BenchmarkDotNet environment block + the three `*-report-github.md` tables from BenchmarkDotNet.Artifacts/results/
- [X] T012 [US2] Create benchmarks/README.md: purpose (Principle V); prerequisites (.NET SDK 10, Release only); run all / filter by class or method / `--job short` for quick checks; benchmark catalog (link to classes); "Compare" section (run on same machine, `--exporters github`, compare Mean and Allocated with baseline/baseline-<version>.md, allocations are the stable signal, what counts as regression to flag in review); "Refresh baseline" section (when performance changes intentionally, new file per core version); note that BenchmarkDotNet refuses non-optimized builds so always use `-c Release`; note that CI only builds, never runs benchmarks

## Phase 5: Polish

- [X] T013 [P] Add a short "Benchmarks" subsection to README.md (root) under Contributing linking to benchmarks/README.md
- [X] T014 (covers SC-003 indirectly: benchmarks are only compiled, never executed, by build/test) Run quickstart.md steps 1–3: build (0 warnings), `dotnet test Viana.Results.slnx -c Release --no-build` (same 336 tests, no benchmark executed), `dotnet pack Viana.Results.slnx -c Release -o <scratch dir>` (no Viana.Results.Benchmarks package)
- [X] T015 Confirm `git status` shows no changes under src/ (FR-008) and no BenchmarkDotNet.Artifacts/ tracked

## Dependencies & Execution Order

- T001 → T002 → T003 → (T004 ∥ T005) → T006 → (T007 ∥ T008 ∥ T009) → T010 → T011 → T012 → (T013 ∥ T014) → T015
- US2 depends on US1 (baseline needs the benchmarks).

## Parallel Example

```text
US1: T007 (ResultBenchmarks.cs) ∥ T008 (ProblemBenchmarks.cs) ∥ T009 (JsonBenchmarks.cs)
```

## Implementation Strategy

1. MVP: Phases 1–3 (benchmarks runnable).
2. US2: baseline + README.
3. Polish: root README link and final validation.
