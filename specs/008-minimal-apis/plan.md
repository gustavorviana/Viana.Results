# Implementation Plan: Minimal APIs Support

**Branch**: `chore/add-spec-kit` (shared) | **Date**: 2026-11-22 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/008-minimal-apis/spec.md`

## Summary

- **Runtime** (`Viana.Results.AspNetCore`): extract the failure/success logic of `VianaActionResult` into an
  internal `ResultResponseWriter` shared by MVC and minimal APIs. New `VianaHttpResult`
  (`Microsoft.AspNetCore.Http.IResult`), `ToHttpResult()` (+ `Task<>` overload), and an endpoint filter
  (`VianaResultEndpointFilter`, `AddVianaResults()` on route handler/group builders).
- **Docs** (`Viana.Results.OpenApi`): endpoint metadata `VianaResponseMetadata` added by
  `ProducesResult(...)` / `ProducesProblemResult(...)`; operation processing reads endpoint metadata
  (native and Swashbuckle) and builds responses at documentation time, generating success schemas through a
  pipeline-provided schema factory. Result-returning example providers (`IExampleProvider<Result<T>>`, list,
  paged, failed) serialized with `ResultResponseBody`; `[ResponseExample(typeof(Provider))]` without status.

## Technical Context

**Language/Version**: C# latest, .NET SDK 10

**Primary Dependencies**: none new (ASP.NET Core shared framework, Microsoft.AspNetCore.OpenApi, Swashbuckle as today)

**Storage**: N/A

**Testing**: xUnit; minimal API endpoints in the existing in-memory `TestApp` (tests/Viana.Results.AspNetCore.Tests); OpenAPI unit tests with endpoint metadata + example apps for both pipelines

**Target Platform**: AspNetCore package net8.0/net10.0; OpenApi/Swashbuckle net10.0

**Project Type**: libraries (NuGet)

**Performance Goals**: minimal API writing within Principle V (benchmark `VianaHttpResult` next to `VianaActionResult`)

**Constraints**: TreatWarningsAsErrors; no new package dependency; controller behavior unchanged

**Scale/Scope**: ~8 new source files, ~45 tests, README/example updates

## Constitution Check

| Principle | Status | Note |
|-----------|--------|------|
| I. Minimal core | PASS | Core untouched |
| II. TFMs | PASS | Same targets as the packages |
| III. TDD | PASS | Parity (controller vs minimal) and documentation tests first |
| IV. RFC 9457 | PASS | Same problem pipeline as feature 006 |
| V. Performance | PASS | Benchmark added |
| VII. SemVer | PASS | Additive; AspNetCore 1.0.0, OpenApi 1.1.0, Swashbuckle 1.1.2 are unreleased and absorb it |
| VIII. Docs | PASS | READMEs + minimal API endpoint in the example apps |
| IX. Native interop | PASS | Native problem pipeline and native schema generators |

Post-design re-check: PASS.

## Project Structure

```text
specs/008-minimal-apis/ (plan, research, data-model, quickstart, contracts/public-api.md, tasks)

src/Viana.Results.AspNetCore/
├── ResultResponseWriter.cs          # internal: shared failure (problem) and success writing
├── VianaActionResult.cs             # MVC, now delegates to the writer
├── VianaHttpResult.cs               # minimal API IResult
├── HttpResultExtensions.cs          # ToHttpResult()
└── Endpoints/VianaResultEndpointFilter.cs, EndpointConventionExtensions.cs (AddVianaResults)
src/Viana.Results.OpenApi/
├── Endpoints/VianaResponseMetadata.cs        # endpoint metadata (status, data type, provider type, inline value, problem info)
├── Endpoints/RouteHandlerBuilderExtensions.cs # ProducesResult / ProducesProblemResult
├── ResponseExampleAttribute.cs                # + ctor without status
├── ExampleResolver.cs                         # result-returning providers, metadata examples
├── Processing/OperationProcessor.cs           # ApplyEndpointDeclarations (metadata), status checks
└── Transformers/*, ../Viana.Results.OpenApi.Swashbuckle/Filters/* # pass endpoint metadata + schema factory
tests/Viana.Results.AspNetCore.Tests/MinimalApi*.cs, tests/Viana.Results.OpenApi.Tests/EndpointDeclarationTests.cs, ResultExampleTests.cs
examples/ (one minimal API endpoint per example host)
```

**Structure Decision**: Existing projects only.

## Complexity Tracking

None.
