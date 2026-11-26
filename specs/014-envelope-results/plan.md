# Implementation Plan: Envelope results

**Branch**: `chore/add-spec-kit` | **Date**: 2026-11-26 | **Spec**: [spec.md](spec.md)

## Summary

The core gains `IEnvelopeResult<T> : IResult<T>`. Every package already funnels the response body through
`ResultResponseBody.GetBody` and the unwrap decision through `ResultReflections.IsUnwrappableType`, so the rule
lives in the core:

- an envelope is never unwrapped;
- its body is built as a JSON object from the application's serializer metadata, without `Status`, `Problem`
  and `Extensions`, and with the extensions flattened at the root;
- `PagedResult<T>` becomes an envelope, and the `IPagedResult<>` special case is removed;
- the OpenAPI schema processors describe the same members, and `additionalProperties` is allowed when the type
  has extensions.

## Technical Context

**Language/Version**: C# latest; core `netstandard2.0`/`net8.0`/`net10.0`, Mvc `net6.0`+, AspNetCore `net8.0`+,
OpenApi `net10.0`.

**Primary Dependencies**: System.Text.Json, which the core already references for `ProblemResultJsonConverter`.

**Testing**: xUnit in `tests/Viana.Results.Tests`, `Viana.Results.Mvc.Tests`, `Viana.Results.AspNetCore.Tests`,
`Viana.Results.OpenApi.Tests` and `Viana.Results.Examples.Tests`.

**Project Type**: library.

**Performance Goals**: the response path of non-envelope results must not change. Envelopes pay one
serialization to a JSON node. A benchmark is required because this is a hot path (constitution V).

**Constraints**: core trimming/AOT friendly: no `RequiresUnreferencedCode` APIs, only `JsonTypeInfo` metadata.
TreatWarningsAsErrors. One type per file in tests and examples. READMEs stay minimal.

**Scale/Scope**: about 6 source files in the core and satellites, tests in 4 projects, and 1 example.

## Constitution Check

| Principle | Status |
|---|---|
| I Minimal core | ✅ The core gains one interface and body shaping with System.Text.Json, which is already a dependency. |
| II Compatibility | ✅ `netstandard2.0` is kept. The `JsonTypeInfo` APIs are available on every target through the System.Text.Json package. |
| III Test-first | ✅ Red tests per story. |
| IV RFC 9457 | ✅ Problems are unchanged. |
| V Performance | ⚠️ The serialization path changes. A before/after benchmark of `PagedResult` and `Result<T>` writes is required (task). |
| VI AOT (core) | ⚠️ The new code avoids annotated APIs. The repository has no AOT smoke test yet (pre-existing gap); see R5. |
| VII Breaking | ✅ The output is unchanged for existing types. Nothing is published. |
| VIII Docs | ✅ XML docs on the new API, release notes, a short README snippet, and the example (FR-010). |
| IX Native interop | ✅ AspNetCore still writes through `ObjectResult` and `HttpResults.Json`. Only the body object changes. |

## Project Structure

```text
src/Viana.Results/
├── IEnvelopeResult.cs              # NEW public interface (XML docs)
├── EnvelopeBody.cs                 # NEW internal: builds the envelope JSON object from JsonTypeInfo
├── ResultResponseBody.cs           # GetBody(result, options) overload; envelope branch
├── ResultReflections.cs            # IsEnvelopeType; IsUnwrappableType ignores envelopes; IPagedResult case removed
└── PagedResult.cs                  # implements IEnvelopeResult<IReadOnlyList<T>>
src/Viana.Results.Mvc/VianaResultAction.cs           # passes its JsonSerializerOptions
src/Viana.Results.AspNetCore/VianaActionResult.cs    # passes MVC JsonOptions
src/Viana.Results.AspNetCore/VianaHttpResult.cs      # passes Http JsonOptions
src/Viana.Results.OpenApi/Processing/SchemaProcessor  # omit extensions, additionalProperties for envelopes
src/Viana.Results.OpenApi/Processing/OperationProcessor.cs  # examples use the same body
examples/…/CursorResult.cs (+ endpoint)               # FR-010
```

## Complexity Tracking

| Item | Why | Simpler alternative rejected |
|---|---|---|
| Body built as a JSON node | Needed to drop interface members and flatten extensions without reflection | Returning the object as-is: it would write `status`/`problem`/`extensions` for envelopes that do not derive from `Result` |
