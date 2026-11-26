# Implementation Plan: Bug-fix Sweep — Results, Mvc and OpenApi

**Branch**: `chore/add-spec-kit` (shared) | **Date**: 2026-11-22 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/007-bugfix-results-mvc-openapi/spec.md`

## Summary

Fix 16 review items across three packages, each with a regression test written first:

- **Core** (`src/Viana.Results`): `ResultResponseBody.GetBody` (I1 null body, MV3 problem before 204
  check), `ResultReflections.IsScalarLike` (I8 explicit simple-type list), `ProblemResult.NormalizeExtensions`
  (I9 always read-only).
- **Mvc** (`src/Viana.Results.Mvc`): `VianaResultAction` (MV1 problem content type, MV2 stream with
  `JsonSerializer.SerializeAsync` + `RequestAborted`), `AddVianaResultFilter` (MV4 idempotent).
- **OpenApi** (`src/Viana.Results.OpenApi`, `.Swashbuckle`): example node built as real JSON (B1),
  default title from the core reason phrase (B2), `ValueTask` unwrapping (B4), `[AllowAnonymous]` and
  endpoint authorization metadata (B5), `[Obsolete]` on internal helpers (M1), options pattern (M2),
  `ActivatorUtilities` (M3), name collisions (M5), Swashbuckle unwrap keeps media type (M6).

## Technical Context

**Language/Version**: C# latest, .NET SDK 10

**Primary Dependencies**: none new

**Storage**: N/A

**Testing**: xUnit in `tests/Viana.Results.Tests`, `tests/Viana.Results.Mvc.Tests`, `tests/Viana.Results.OpenApi.Tests`

**Target Platform**: core netstandard2.0/net6.0/net8.0/net10.0; Mvc net6.0/net8.0/net10.0; OpenApi net10.0

**Project Type**: libraries (NuGet)

**Performance Goals**: MV2 lowers allocations of `VianaResultAction` (new benchmark, Principle V)

**Constraints**: TreatWarningsAsErrors; no API removals; behavior changes only those listed in the spec

**Scale/Scope**: ~10 source files, ~35 new/adjusted tests, release notes for 4 packages

## Constitution Check

| Principle | Status | Note |
|-----------|--------|------|
| I. Minimal core | PASS | No new dependency |
| II. netstandard2.0 | PASS | Core fixes use APIs available on netstandard2.0 |
| III. TDD | PASS | One failing regression test per item (FR-017) |
| IV. RFC 9457 | PASS | MV1 fixes the problem media type |
| V. Performance | PASS | MV2 benchmark before/after; no regressions elsewhere |
| VII. SemVer | PASS | Owner decisions: core 3.0.0 (unreleased), Mvc 2.1.0, OpenApi 1.1.0 / Swashbuckle 1.1.2 (unreleased) |
| VIII. Docs | PASS | Package READMEs and release notes |

Post-design re-check: PASS.

## Project Structure

### Documentation (this feature)

```text
specs/007-bugfix-results-mvc-openapi/
├── plan.md, research.md, data-model.md, quickstart.md
├── contracts/behavior-changes.md
└── tasks.md (next)
```

### Source Code

```text
src/Viana.Results/ResultResponseBody.cs            # I1, MV3
src/Viana.Results/ResultReflections.cs             # I8
src/Viana.Results/ProblemResult.cs                 # I9
src/Viana.Results/Viana.Results.csproj             # release notes 3.0.0
src/Viana.Results.Mvc/VianaResultAction.cs         # MV1, MV2
src/Viana.Results.Mvc/VianaResultFilterExtensions.cs  # MV4
src/Viana.Results.Mvc/Viana.Results.Mvc.csproj     # 2.1.0 + release notes
src/Viana.Results.OpenApi/Schemas/ProblemResultSchema.cs, ProblemFieldSchema.cs   # B1, B2, M5
src/Viana.Results.OpenApi/Processing/OpenApiOperationProcessor.cs                 # B4, B5
src/Viana.Results.OpenApi/Transformers/*.cs        # pass endpoint metadata (B5)
src/Viana.Results.OpenApi/ResponseExampleResolver.cs, VianaResultOptions.cs, ServiceCollectionExtensions.cs  # M2, M3
src/Viana.Results.OpenApi/Processing/*.cs, ResponseExampleResolver.cs, Schemas/*.cs  # M1 [Obsolete]
src/Viana.Results.OpenApi.Swashbuckle/Filters/*.cs # B5 metadata, M6
tests/...                                          # regression tests per item
benchmarks/Viana.Results.Benchmarks/MvcBenchmarks.cs  # MV2
```

**Structure Decision**: Existing projects only.

## Complexity Tracking

None.
