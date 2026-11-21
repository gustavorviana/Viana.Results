# Implementation Plan: Warnings as Errors

**Branch**: `chore/add-spec-kit` (shared branch) | **Date**: 2026-11-21 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/001-warnings-as-errors/spec.md`

## Summary

Make every project in the repository fail its build on any warning (compiler, nullable, XML docs,
analyzers, NuGet vulnerability audit), defined once in a root `Directory.Build.props`. Fix the
existing warnings at the source: pin the vulnerable transitive `Microsoft.OpenApi` to a clean 2.x
version via CPM transitive pinning, enable nullable warnings in tests and fix them, and remove the
null-forgiving operators in the core JSON converter using RFC 9457 defaults. The only remaining
exemption is CS1591 in example projects, justified inline. Details: [research.md](./research.md).

## Technical Context

**Language/Version**: C# latest, .NET SDK 10.0.x

**Primary Dependencies**: None added. Existing: System.Text.Json (core), Microsoft.AspNetCore.App,
Microsoft.AspNetCore.OpenApi 10.0.3, Swashbuckle.AspNetCore 10.1.4; `Microsoft.OpenApi` pinned to
2.13.1 (transitive)

**Storage**: N/A

**Testing**: xUnit (existing test projects in `tests/`)

**Target Platform**: Packages: netstandard2.0, net6.0, net8.0, net10.0 (varies per package);
CI: GitHub Actions, ubuntu-latest

**Project Type**: Library (multi-package NuGet repository)

**Performance Goals**: N/A (build configuration only; no runtime hot path touched except a
null-default branch in deserialization)

**Constraints**: Zero warnings across all TFMs; no new dependencies; no public API signature
changes and no behavior changes (FR-010 only removes null-forgiving operators)

**Scale/Scope**: 5 packages, 4 test projects, 3 example projects; 1 vulnerable package,
5 nullable warnings in tests, 2 null-forgiving operators, 1 exemption to justify

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Rule | Status | Notes |
|---|---|---|
| I. Minimal, Portable Core | ✅ | No dependency added to the core. |
| II. Broad Compatibility | ✅ | No TFM change; every TFM must build warning-free. |
| III. Test-First | ✅ | Converter refactor (R5) is guarded by characterization tests for missing `title`/`type` written first. Build rule verified by a deliberate-warning check (quickstart). |
| IV. RFC 9457 | ✅ | Current defaults (`about:blank`, `Error`) preserved; reason-phrase alignment deferred to its own feature. |
| V. Performance & Simplicity | ✅ | Not a hot path change in practice (only the absent-member branch); no benchmark required. |
| VI. AOT (core) | ✅ | No reflection added. Prepares the ground: AOT analyzer warnings will become errors. |
| VII. SemVer | ✅ | No breaking change. OpenApi packages gain a `Microsoft.OpenApi >= 2.13.1` dependency (patch/minor-level change). |
| VIII. Documented Surface | ✅ | README gets the zero-warning rule; no public API added. |
| IX. Native Interop | N/A | |
| Technical Constraints: `TreatWarningsAsErrors`, `Nullable`, CPM | ✅ | This feature implements them. |
| Workflow: Conventional Commits, CI green | ✅ | |

**Post-design re-check**: ✅ No violations. Complexity Tracking not needed.

## Project Structure

### Documentation (this feature)

```text
specs/001-warnings-as-errors/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (build policy model)
├── quickstart.md        # Phase 1 output (validation guide)
├── contracts/
│   └── build-policy.md  # Phase 1 output (the rule contributors must follow)
└── tasks.md             # Phase 2 output (/speckit-tasks; not created here)
```

### Source Code (repository root)

```text
Directory.Build.props                 # NEW: TreatWarningsAsErrors, Nullable, NuGetAuditMode
Directory.Packages.props              # EDIT: transitive pinning + Microsoft.OpenApi 2.13.1
examples/Directory.Build.props        # NEW: imports root; CS1591 exemption, justified inline
examples/Viana.Results.Examples.Shared/*.csproj   # EDIT: remove local NoWarn
src/Viana.Results/ProblemResultJsonConverter.cs   # EDIT: RFC 9457 defaults instead of `!`
tests/*/*.Tests.csproj                # EDIT: remove Nullable=annotations
tests/Viana.Results.Tests/**          # EDIT: fix CS8600; add converter default tests
tests/Viana.Results.OpenApi.Tests/**  # EDIT: fix CS8600/CS8604
README.md                             # EDIT: zero-warnings contributing rule
.github/workflows/ci.yml              # unchanged (enforced via Directory.Build.props)
```

**Structure Decision**: Existing multi-package layout (`src/`, `tests/`, `examples/`) is kept.
The rule is centralized in root-level MSBuild files so it applies to all three folders.

## Complexity Tracking

Not applicable — no constitution violations.
