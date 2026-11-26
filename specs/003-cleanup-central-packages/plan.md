# Implementation Plan: Clean Up Unused Central Package Entries

**Branch**: `chore/add-spec-kit` (shared) | **Date**: 2026-11-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/003-cleanup-central-packages/spec.md`

## Summary

Remove the 4 unreferenced `PackageVersion` entries and the empty `<ItemGroup>` from
`Directory.Packages.props`, then prove restore/build/test still pass with zero warnings. No source,
project or package metadata changes.

## Technical Context

**Language/Version**: MSBuild / .NET SDK 10, Central Package Management

**Primary Dependencies**: N/A (removal only)

**Storage**: N/A

**Testing**: existing xUnit suites (306+30 tests) on every TFM; NuGet audit mode `all`

**Target Platform**: repository build (local + GitHub Actions CI)

**Project Type**: library repository maintenance

**Performance Goals**: N/A

**Constraints**: TreatWarningsAsErrors; no published package changes (FR-005)

**Scale/Scope**: 1 file, 5 lines removed

## Constitution Check

| Principle | Status | Note |
|-----------|--------|------|
| I. Minimal core | PASS | Reduces declared dependency surface |
| III. TDD | PASS (N/A) | No behavior change; existing suite is the regression gate |
| VII. SemVer | PASS | No package version change |
| Technical constraints (warnings as errors) | PASS | Validated by quickstart |
| Workflow (Conventional Commits) | PASS | `build(deps): ...` commit |

Post-design re-check: PASS.

## Project Structure

```text
specs/003-cleanup-central-packages/
├── plan.md, research.md, data-model.md, quickstart.md
├── contracts/central-packages.md
└── tasks.md (next)
Directory.Packages.props   # only file changed
```

**Structure Decision**: Single-file change at repository root.

## Complexity Tracking

None.
