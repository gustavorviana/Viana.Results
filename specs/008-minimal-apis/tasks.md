# Tasks: Minimal APIs Support

**Input**: Design documents from `specs/008-minimal-apis/`

**Prerequisites**: plan.md, spec.md, research.md (R1–R7), data-model.md, contracts/public-api.md

**Tests**: Required (constitution Principle III). Tests first; they must fail before the implementation.

## Phase 1: Setup

- [X] T001 Record baseline: `dotnet build Viana.Results.slnx -c Release` (0 warnings) and `dotnet test Viana.Results.slnx -c Release` (550 tests passing)

## Phase 2: Foundational

- [X] T002 Refactor (no behavior change) src/Viana.Results.AspNetCore/VianaActionResult.cs: move problem writing (mapper, native trace id, `IProblemDetailsService.TryWriteAsync`, fallback hook) into internal `ResultResponseWriter` in src/Viana.Results.AspNetCore/ResultResponseWriter.cs with `WriteProblemAsync(HttpContext, Viana.Results.IResult, JsonNamingPolicy?, Func<ProblemDetails, Task> fallback)`; MVC passes the `ProblemDetailsFactory` + `ObjectResult` fallback; all 126 AspNetCore tests stay green

## Phase 3: User Story 1 – Return Viana results from minimal API endpoints (P1) 🎯 MVP

**Goal**: `ToHttpResult()`, `VianaHttpResult`, `AddVianaResults()` endpoint filter; responses identical to controllers.

**Independent Test**: the same failed and successful results returned from a controller action and from a minimal endpoint produce identical status, content type and body (excluding `traceId` value).

### Tests (write first, must fail)

- [X] T003 [P] [US1] Create tests/Viana.Results.AspNetCore.Tests/MinimalApiParityTests.cs: register minimal endpoints in `TestApp` (via `TestAppOptions.Endpoints`) mirroring the same cases as `CasesController`/`SuccessController` (feature 006) (explicit problem, trace, untitled 422, non-error 200 problem, status differs, failed Result/Result<T>/ListResult/PagedResult, success value, created 201, no content 204, null value, list, paged) using `ToHttpResult()`; assert each response equals the controller one (status, media type, JSON members except `traceId`); with and without `AddProblemDetails`; a null value returns 204 (research R2)
- [X] T004 [P] [US1] Create tests/Viana.Results.AspNetCore.Tests/MinimalApiFilterTests.cs: with `.AddVianaResults()` on an endpoint and on a `MapGroup`, handlers returning `Result<T>` (sync) and `Task<Result<T>>` (async) equal `ToHttpResult()`; handlers returning non-Viana values unchanged; minimal API JSON naming (`Microsoft.AspNetCore.Http.Json.JsonOptions`, camelCase) honored for success bodies and problem extension names

### Implementation

- [X] T005 [US1] Implement src/Viana.Results.AspNetCore/VianaHttpResult.cs (`public sealed class VianaHttpResult(Viana.Results.IResult result) : Microsoft.AspNetCore.Http.IResult`): failure → `ResultResponseWriter.WriteProblemAsync` with `Http.Json.JsonOptions` naming policy and `TypedResults.Problem(details)` fallback; success → `!HasBody` → `TypedResults.StatusCode(status)`; null value → 204; otherwise `TypedResults.Json(body, statusCode: status)`
- [X] T006 [US1] Implement src/Viana.Results.AspNetCore/HttpResultExtensions.cs (`ToHttpResult(this Viana.Results.IResult)`, `ToHttpResult<TResult>(this Task<TResult>)`) and src/Viana.Results.AspNetCore/Endpoints/VianaResultEndpointFilter.cs (`IEndpointFilter` wrapping a returned Viana result in `VianaHttpResult`) plus src/Viana.Results.AspNetCore/Endpoints/VianaEndpointConventionExtensions.cs (`AddVianaResults<TBuilder>(this TBuilder) where TBuilder : IEndpointConventionBuilder` → `AddEndpointFilter`); XML docs; make T003–T004 pass

**Checkpoint**: US1 complete.

## Phase 4: User Story 2 – Document minimal API responses (P1)

**Goal**: `ProducesResult` / `ProducesProblemResult` read by both pipelines.

**Independent Test**: endpoint metadata produced by the extensions, processed by the internal operation processor with a fake schema factory, yields the declared responses, schemas and examples; both example apps show the minimal endpoint correctly.

### Tests (write first, must fail)

- [X] T007 [P] [US2] Create tests/Viana.Results.OpenApi.Tests/EndpointDeclarationTests.cs: build a minimal endpoint metadata list via the extensions on a `RouteHandlerBuilder` from a test `WebApplication` (or an `IEndpointConventionBuilder` stub collecting metadata) and process it with `OperationProcessor.ApplyEndpointDeclarations(operation, metadata, json, services, schemaFactory)`: `ProducesResult<User>()` → 200 with factory schema for `User`; `ProducesResult<UserOkExample>()` (provider returning `Result<User>`) → 200 example = unwrapped user; `ProducesResult(Results.Ok(new User()))` inline → same; `ProducesProblemResult(404, title, type, detail)` → 404 `application/problem+json` shared schema with those values; `ProducesProblemResult<NotFoundExample>()` and `(typeof(NotFoundExample))` and inline failed result → status from the result; explicit status ≠ result status → `InvalidOperationException` naming both statuses; two problem examples for one status → union schema + both examples; precedence endpoint example > endpoint declaration > global example; a success example whose value is null documents a 204 response without body (spec edge case)
- [X] T008 [P] [US2] Extend tests/Viana.Results.OpenApi.Tests/ProblemSourcePrecedenceTests.cs to confirm controllers (attributes) are unchanged by the new metadata path

### Implementation

- [X] T009 [US2] Create src/Viana.Results.OpenApi/Endpoints/VianaResponseMetadata.cs (fields per data-model.md) and src/Viana.Results.OpenApi/Endpoints/VianaRouteHandlerBuilderExtensions.cs (signatures from contracts/public-api.md; `ProducesResult<T>()` auto-detects `IExampleProvider<>`)
- [X] T010 [US2] In src/Viana.Results.OpenApi/Processing/OperationProcessor.cs add internal `ApplyEndpointDeclarations(...)` implementing research R4/R5 (status resolution, mismatch error, problem responses via `ProblemResultSchema`, success responses via the schema factory, examples with `ResultResponseBody` rules, precedence with the existing attribute/global logic)
- [X] T011 [US2] Make `ApplyEndpointDeclarations` async (`ValueTask`) with a `Func<Type, ValueTask<IOpenApiSchema>>` schema factory (research R5, option C). Native: await it from src/Viana.Results.OpenApi/Transformers/ResponseExampleOperationTransformer.cs (and ProblemResponseOperationTransformer.cs when needed) with `type => new(context.GetOrCreateSchemaAsync(type, null, cancellationToken))`. Swashbuckle: convert src/Viana.Results.OpenApi.Swashbuckle/Filters/ResponseExampleOperationFilter.cs (and ProblemResponseOperationFilter.cs when needed) to `IOperationAsyncFilter` with `type => ValueTask.FromResult(context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository))` and register them via `options.OperationAsyncFilter<T>()` in src/Viana.Results.OpenApi.Swashbuckle/SwaggerGenExtensions.cs; no `.Result`/`.Wait()`/`GetAwaiter().GetResult()` in library code; make T007–T008 pass
- [X] T012 [US2] Verify the synchronous Swashbuckle path: a test resolving `ISwaggerProvider` and calling `GetSwagger("v1")` on a host with a minimal endpoint using `ProducesResult` documents the same responses as the async path (`GetSwaggerAsync` / middleware); if async filters are skipped there, add a synchronous filter for that path using only the synchronous generator (never waiting on async code) and record the outcome in research.md R5

**Checkpoint**: US1 and US2 complete.

## Phase 5: User Story 3 – Example classes returning real results (P2)

**Goal**: result-returning providers everywhere; `[ResponseExample(typeof(Provider))]` without status.

**Independent Test**: a controller action with `[ResponseExample(typeof(UserOkExample))]` documents 200 with the unwrapped user; a failed-result provider documents a problem response.

- [X] T013 [P] [US3] Create tests/Viana.Results.OpenApi.Tests/ResultExampleTests.cs: providers returning `Result<User>`, `ListResult<User>`, `PagedResult<User>`, failed `Result` → examples equal `ResultResponseBody.GetBody` serialization and statuses from the result; `[ResponseExample(typeof(P))]` without status works on controllers; `[ResponseExample(201, typeof(P))]` with a 200 result → error; raw-value providers unchanged
- [X] T014 [US3] In src/Viana.Results.OpenApi/ResponseExampleAttribute.cs add `public ResponseExampleAttribute(Type providerType)` (status resolved later; public `StatusCode` stays `int`, `0` means "taken from the provider result" — document it in the XML doc of `StatusCode` and in the OpenApi README); in src/Viana.Results.OpenApi/ExampleResolver.cs and OperationProcessor.cs resolve status from result providers and serialize via `ResultResponseBody.GetBody`; make T013 pass

**Checkpoint**: All stories complete.

## Phase 6: Polish

- [X] T015 [P] Add a minimal API sample endpoint to both example hosts (examples/Viana.Results.Scalar/Program.cs, examples/Viana.Results.Swagger/Program.cs) with `.AddVianaResults().ProducesResult<...>().ProducesProblemResult<...>()` using example classes in examples/Viana.Results.Examples.Shared/Examples/; confirm the endpoint appears in both OpenAPI documents and add `builder.Services.AddEndpointsApiExplorer()` to the Scalar host if it does not
- [X] T016 [P] READMEs: src/Viana.Results.AspNetCore/README.md (minimal APIs section: `ToHttpResult`, `AddVianaResults`, null → 204), src/Viana.Results.OpenApi/README.md and src/Viana.Results.OpenApi.Swashbuckle/README.md (`ProducesResult`/`ProducesProblemResult`, result-returning providers, `[ResponseExample]` without status); release notes in the three csproj files (no version bump, research R7)
- [X] T017 [P] Add `MinimalSuccess`/`MinimalFailure` to benchmarks/Viana.Results.Benchmarks/AspNetCoreBenchmarks.cs and record in benchmarks/Viana.Results.Benchmarks/baseline/aspnetcore-1.0.0.md
- [X] T018 Run quickstart.md steps 1–5 (build, tests, both example apps runtime + OpenAPI documents for the minimal endpoint, benchmark)

## Dependencies & Execution Order

- T001 → T002 → US1 ((T003 ∥ T004) → T005 → T006) → US2 ((T007 ∥ T008) → T009 → T010 → T011 → T012) → US3 (T013 → T014) → (T015 ∥ T016 ∥ T017) → T018
- US2/US3 do not depend on US1 code (OpenAPI package only) and could run in parallel with it.

## Parallel Example

```text
US1 tests: T003 ∥ T004
US2 tests: T007 ∥ T008
Polish:    T015 ∥ T016 ∥ T017
```

## Implementation Strategy

1. MVP: US1 (runtime parity).
2. US2: documentation helpers.
3. US3: result-returning examples.
4. Polish: samples, docs, benchmark, validation.
