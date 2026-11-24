# Implementation Plan: Bodyless Result is 204, and WithoutDefaultResult

**Branch**: `chore/add-spec-kit` | **Date**: 2026-11-24 | **Spec**: [spec.md](spec.md)

## Summary

The work has two parts.

**Runtime.** `Results.Ok()` without a value returns `new Result(204)`. The writers already write a bodyless
success with its own status (`ResultResponseBody.HasBody`), so no writer changes.

**Documentation.** The OpenAPI transformer and Swashbuckle filter for bare results (`ClearBodyForBareResult`)
become a single step, `ApplyDefaultResult`, shared by both generators. For each operation it does three things:

1. It works out which successes were declared by hand.
2. It removes the success inferred from the return type when that success was not declared by hand (the 200
   that ASP.NET Core infers for minimal APIs and for MVC actions without `[ProducesResponseType]`).
3. For a bare `Result` with no success declared by hand, it adds a `204 No Content` response.

`WithoutDefaultResult` is a marker. It is set with the `[WithoutDefaultResult]` attribute or the
`.WithoutDefaultResult()` metadata. On a marked endpoint, step 3 is skipped and step 2 removes every inferred
success.

## Technical Context

**Language/Version**: C# latest. Core targets netstandard2.0 and net6.0+. OpenApi and Swashbuckle target
net10.0. AspNetCore and Mvc are unchanged.
**Primary Dependencies**: Microsoft.AspNetCore.OpenApi and Swashbuckle.AspNetCore, both already referenced.
**Testing**: xUnit. Unit tests go in `tests/Viana.Results.OpenApi.Tests` (both generators, through
TestServer documents), `tests/Viana.Results.Tests` (`Ok()`) and `tests/Viana.Results.AspNetCore.Tests` and
`Mvc.Tests` (runtime 204). Examples get smoke tests.
**Constraints**: 0 warnings, and no new dependency. Nothing is published, so there is no version bump; the
release notes are updated.
**Scale/Scope**: 1 core line, 1 new attribute, 1 new extension, 1 reworked processor step, about 15 test
updates and 20 new tests, plus the example and README edits.

## Constitution Check

| Principle | Status |
|-----------|--------|
| I. Minimal, Portable Core | ✅ One-line change in the core, no new API or dependency |
| II. Broad Compatibility | ✅ No TFM change; the new API lives in the net10 OpenApi package that already exists |
| III. Test-First | ✅ Each FR gets tests written before the code (tasks order) |
| IV. RFC 9457 | ✅ Problem responses are untouched; only successes change |
| V. Performance and Simplicity | ✅ Document-time only. Runtime cost is unchanged (one status constant) |
| VI. AOT (Core) | ✅ No reflection added to the core |
| VII. SemVer | ✅ Nothing is published (spec FR-012). The behavior change goes in the release notes |
| VIII. Documented Public Surface | ✅ XML docs for the attribute and the extension; READMEs updated |
| IX. Native Interop | ✅ Uses native endpoint metadata (`IProducesResponseTypeMetadata`) and MVC `[ProducesResponseType]` |

Gate: PASS. Re-checked after design: PASS.

## Project Structure

```text
src/Viana.Results/Results.cs                                   # Ok() → new Result(204); XML doc
src/Viana.Results.OpenApi/
├── WithoutDefaultResultAttribute.cs                           # new, [AttributeUsage(Method|Class)]
├── Endpoints/VianaRouteHandlerBuilderExtensions.cs            # + WithoutDefaultResult() (endpoint, group, convention)
├── Processing/OperationProcessor.cs                           # ClearBodyForBareResult → ApplyDefaultResult(operation, method, metadata)
├── Processing/OpenApiOperationProcessor.cs                    # public wrapper renamed accordingly
└── Transformers/NoResponseBodyOperationTransformer.cs         # passes endpoint metadata
src/Viana.Results.OpenApi.Swashbuckle/Filters/NoResponseBodyOperationFilter.cs  # same, from ApiDescription
tests/Viana.Results.OpenApi.Tests/DefaultResultTests.cs        # new (both generators)
tests/Viana.Results.Tests/ResultsHelperTests.cs                # Ok() == 204
tests/... (existing asserts of 200 for Results.Ok())          # updated
examples/...                                                   # US3
```

**Structure Decision**: the attribute and the extension go in Viana.Results.OpenApi, next to
`ProducesResult`/`[ResponseExample]`. Swashbuckle already references that package, so both generators share one
implementation.

## Complexity Tracking

None.
