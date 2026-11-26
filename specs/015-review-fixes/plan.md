# Implementation Plan: Review fixes

**Branch**: `chore/add-spec-kit` | **Date**: 2026-11-26 | **Spec**: [spec.md](spec.md)

## Summary

The plan has three small, independent fixes and one cleanup:

1. `EnvelopeBody` decides whether to shape the body from the contract: whether a container member can be written.
   It no longer looks at the output of the first instance.
2. `ApplyAuthorizeResponses` creates the responses collection when it is missing.
3. `ApplyResponseExamples` no longer returns early on an empty operation, so problem examples still create their
   own response.
4. Announcements of future versions are removed outside `specs/`.

## Technical Context

**Language/Version**: C# latest, existing TFMs.

**Primary Dependencies**: System.Text.Json 8 contract metadata (`JsonPropertyInfo`), Microsoft.OpenApi.

**Testing**: xUnit, in `tests/Viana.Results.Tests` and `tests/Viana.Results.OpenApi.Tests`.

**Project Type**: library.

**Performance Goals**: the allocation of the 014 "after" benchmark is kept (SC-002).

**Constraints**: TreatWarningsAsErrors, no new AOT warnings, one type per file in tests.

**Scale/Scope**: 2 source files, release notes, and the text of the files found by the version search.

## Constitution Check

| Principle | Status |
|---|---|
| I–II | ✅ No change of dependencies or TFMs |
| III Test-first | ✅ Red tests for US1 and US2 |
| IV RFC 9457 | ✅ Unchanged |
| V Performance | ✅ The decision stays cached per contract. The benchmark is rerun and compared with `envelope-after.md` |
| VI AOT | ✅ Only `JsonPropertyInfo` and attribute reads. The AOT analyzer check is rerun |
| VII Breaking | ✅ Obsolete messages change text only. Nothing is published |
| VIII Docs | ✅ Release notes. No README change unless one carries a version announcement |
| IX Interop | ✅ Unchanged |

## Project Structure

```text
src/Viana.Results/EnvelopeBody.cs                       # contract-based decision (R1)
src/Viana.Results.OpenApi/Processing/OperationProcessor.cs  # Responses ??= (R2, R3)
files found by the version search (R4)                  # text only
tests/Viana.Results.Tests/Envelopes/                    # new conditional-status envelope type + tests
tests/Viana.Results.OpenApi.Tests/EmptyResponses/       # new tests
```

There are no contracts or data model to change: the public API is unchanged.

## Complexity Tracking

None.
