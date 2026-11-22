# Implementation Plan: RFC 9457 "detail" and Builder Results

**Branch**: `chore/add-spec-kit` (shared) | **Date**: 2026-11-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/005-problem-detail-builder-results/spec.md`

## Summary

Core change in `src/Viana.Results` plus a small OpenAPI annotation change (US3):

- Add `ProblemResult.WithDetail(status, title, detail, type, extensions)` (stores `extensions["detail"]`).
- Mark `ProblemResult.WithDescription` and `ProblemBuilder.WithDescription` `[Obsolete]` with the
  message from [contracts/public-api.md](contracts/public-api.md) (removal in 4.0.0).
- Add `ProblemBuilder.BuildResult()` → `Result` and `BuildResult<T>()` → `Result<T>`.
- Internal callers (`ProblemBuilder.Build`, `ProblemResultJsonConverter`) move to a non-obsolete
  internal factory so the library never calls its own obsolete API.
- Examples migrate to `WithDetail`; tests of obsolete APIs get a narrow `CS0618` suppression.
- Core version 3.0.0 (includes unreleased feature 002).
- OpenAPI: `ProblemResultAttribute.Detail` added and wired into `ProblemResultSchema.FromAttribute`;
  `Description` obsolete (removal in Viana.Results.OpenApi 2.0.0). OpenApi 1.1.0, Swashbuckle 1.1.2.

## Technical Context

**Language/Version**: C# latest; core targets netstandard2.0, net6.0, net8.0, net10.0

**Primary Dependencies**: none new

**Storage**: N/A

**Testing**: xUnit (`tests/Viana.Results.Tests`), all TFMs; Mvc tests compile with suppressions

**Target Platform**: library

**Project Type**: library (NuGet)

**Performance Goals**: `BuildResult` adds one result allocation over `Build()`; no change to existing paths (benchmark `ProblemBenchmarks` before/after)

**Constraints**: TreatWarningsAsErrors (CS0618 becomes an error for internal obsolete use); no behavior change for obsolete APIs

**Scale/Scope**: 3 source files, ~12 tests added/adjusted, README, 2 examples, csproj

## Constitution Check

| Principle | Status | Note |
|-----------|--------|------|
| I. Minimal core | PASS | Additive API only |
| II. netstandard2.0 | PASS | No new framework APIs |
| III. TDD | PASS | Tests for WithDetail/BuildResult/obsolete messages first |
| IV. RFC 9457 | PASS | Promotes the standard `detail` member |
| V. Performance | PASS | `ProblemBenchmarks` + new `BuilderBuildResult` benchmark compared to baseline |
| VII. SemVer | PASS | Owner decision: 3.0.0 now; removal in 4.0.0 announced in the obsolete message |
| VIII. Docs | PASS | XML docs, core README, examples |
| Suppression policy | PASS | `#pragma warning disable CS0618` only around obsolete calls in tests, with comment |

Post-design re-check: PASS.

## Project Structure

### Documentation (this feature)

```text
specs/005-problem-detail-builder-results/
├── plan.md, research.md, data-model.md, quickstart.md
├── contracts/public-api.md
└── tasks.md
```

### Source Code

```text
src/Viana.Results/
├── ProblemResult.cs              # + WithDetail, [Obsolete] WithDescription, internal CreateWithDescription
├── ProblemBuilder.cs             # [Obsolete] WithDescription, + BuildResult(), BuildResult<T>()
├── ProblemResultJsonConverter.cs # uses internal factory
├── Viana.Results.csproj          # Version 3.0.0 + release notes
└── README.md
tests/Viana.Results.Tests/        # ProblemResultTests, ProblemBuilderTests, ProblemResultJsonConverterTests, ObsoleteApiTests
tests/Viana.Results.Mvc.Tests/VianaResultActionTests.cs   # suppression around 3 obsolete calls
examples/Viana.Results.Examples.Shared/Examples/          # InternalServerErrorExample, ServiceUnavailableExample → WithDetail
benchmarks/Viana.Results.Benchmarks/ProblemBenchmarks.cs  # + BuilderBuildResult
src/Viana.Results.OpenApi/ProblemResultAttribute.cs       # + Detail, [Obsolete] Description
src/Viana.Results.OpenApi/Schemas/ProblemResultSchema.cs  # FromAttribute adds "detail" field
src/Viana.Results.OpenApi/Viana.Results.OpenApi.csproj    # 1.1.0 + release notes
src/Viana.Results.OpenApi.Swashbuckle/*.csproj            # 1.1.2 + release notes
src/Viana.Results.OpenApi/README.md, src/Viana.Results.OpenApi.Swashbuckle/README.md
tests/Viana.Results.OpenApi.Tests/                         # ProblemResultAttributeTests, ProblemResultSchemaTests
```

**Structure Decision**: Existing core and OpenAPI projects; no new projects.

## Complexity Tracking

None.
