# Implementation Plan: Validate response declarations against the return type

**Branch**: `chore/add-spec-kit` | **Date**: 2026-11-26 | **Spec**: [spec.md](spec.md)

## Summary

The OpenApi pipelines will check each operation's declared return type before they apply any Viana response
declaration. The check uses a single shared predicate that matches a Viana result or a generic type that carries
one in its type arguments.

- Minimal APIs (US1) and attributes placed directly on an action (US2) fail when the endpoint has no result.
- Attributes placed on the controller class (US3) are applied only to result actions. Each skipped action logs a
  warning, and a controller class with no result action fails.
- Both pipelines run the same rules: the native transformers and the Swashbuckle filters.

## Technical Context

**Language/Version**: C# (latest), packages on net8.0/net10.0 (OpenApi native: net10; Swashbuckle: net8/net10).

**Primary Dependencies**: Microsoft.AspNetCore.OpenApi, Swashbuckle.AspNetCore, Microsoft.Extensions.Logging.

**Testing**: xUnit. The tests are `tests/Viana.Results.OpenApi.Tests` (unit tests on `OperationProcessor` plus
`WebApplicationFactory` hosts) and the Examples tests.

**Project Type**: library (satellite packages).

**Performance Goals**: the check runs only at document generation. Reflection over a controller's actions is
cached per controller type.

**Constraints**: TreatWarningsAsErrors. The OpenApi tests use one type per file. Nothing is published, so the
release notes are the only migration note.

**Scale/Scope**: about 4 transformers, 4 filters, 1 new internal helper, the README and the release notes.

## Constitution Check

| Principle | Status |
|---|---|
| I Minimal core | ✅ Changes are in the OpenApi satellites only. |
| II Compatibility | ✅ No TFM change. |
| III Test-first | ✅ Red tests per story come before each implementation task. |
| IV RFC 9457 | ✅ No change to the problem shape. |
| V Performance/Simplicity | ✅ The checks run at document generation only, outside the request path. No benchmark is required. |
| VI AOT (core) | ✅ The core is untouched. |
| VII Breaking changes | ⚠️ Silent cases become errors. This was approved by the user (see spec). Nothing is published yet, so release notes are enough. |
| VIII Documented surface | ✅ README and release notes are updated. No new public API. |
| IX Native interop | ✅ Only the OpenAPI documents are affected, not responses. |

Re-check after design: no change.

## Project Structure

```text
src/Viana.Results.OpenApi/
├── Processing/ReturnTypeRules.cs              # NEW internal: IsResultReturn, ControllerHasResultAction (cached), Validate
├── Processing/OperationProcessor.cs           # call the rules before Apply* (controllers + minimal)
├── Transformers/*OperationTransformer.cs      # pass controller type / logger
├── README.md
└── Viana.Results.OpenApi.csproj               # release notes
src/Viana.Results.OpenApi.Swashbuckle/
├── Filters/*OperationFilter.cs                # same inputs as the transformers
└── Viana.Results.OpenApi.Swashbuckle.csproj   # release notes
tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/   # NEW, one type per file
```

## Complexity Tracking

None.
