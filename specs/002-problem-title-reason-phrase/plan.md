# Implementation Plan: Problem Title Defaults to Status Reason Phrase

**Branch**: `chore/add-spec-kit` (shared) | **Date**: 2026-11-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-problem-title-reason-phrase/spec.md`

## Summary

When a `ProblemResult` has no title (null, empty, whitespace), default it to the IANA reason phrase
of its status instead of `"Error"`. All three entry points (constructor/`WithDescription`,
`ProblemBuilder`, `ProblemResultJsonConverter`) already funnel through the private `ProblemResult`
constructor, so the change is made in one place. Unknown statuses and table placeholders (306, 418)
keep `"Error"`. Explicit `title` and `type` are never changed. Release as `Viana.Results` 2.1.0.

## Technical Context

**Language/Version**: C# (latest), .NET SDK 10

**Primary Dependencies**: none new (core stays dependency-free besides System.Text.Json)

**Storage**: N/A

**Testing**: xUnit (`tests/Viana.Results.Tests`), all TFMs

**Target Platform**: netstandard2.0, net6.0, net8.0, net10.0

**Project Type**: library (NuGet)

**Performance Goals**: no allocation added; lookup is one dictionary read on the default path only

**Constraints**: TreatWarningsAsErrors; AOT/trimming-safe (no reflection); public API unchanged

**Scale/Scope**: 1 production file + 1 internal helper, ~7 tests updated, ~6 new tests, docs

## Constitution Check

| Principle | Status | Note |
|-----------|--------|------|
| I. Minimal core | PASS | No dependency, no configuration surface (FR-008) |
| II. netstandard2.0 | PASS | Plain dictionary lookup |
| III. TDD | PASS | Tests updated/added first (red), then constructor change |
| IV. RFC 9457 | PASS | Implements §3.1.4 recommendation for `about:blank` |
| V. Performance/simplicity | PASS | Single change point; no extra allocation |
| VI. AOT/trimming | PASS | No reflection |
| VII. SemVer | PASS | Discussed with owner: MINOR 2.1.0 + release notes |
| VIII. Docs | PASS | XML docs, core README and release notes updated |
| IX. Framework interop | PASS | Customization stays in host (`CustomizeProblemDetails`) |

Post-design re-check: PASS (no change).

## Project Structure

### Documentation (this feature)

```text
specs/002-problem-title-reason-phrase/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/default-title.md
└── tasks.md   (next: /speckit-tasks)
```

### Source Code

```text
src/Viana.Results/
├── ProblemResult.cs              # constructor: title default → reason phrase
├── HttpStatusInfo.cs             # internal TryGetReasonPhrase (excludes placeholders)
├── ProblemResultJsonConverter.cs # comment update only
├── Viana.Results.csproj          # Version 2.1.0 + release notes
└── README.md                     # document default title
tests/Viana.Results.Tests/
├── ProblemResultTests.cs
├── ProblemBuilderTests.cs
└── ProblemResultJsonConverterTests.cs
```

**Structure Decision**: Existing core project; no new projects.

## Complexity Tracking

None.
