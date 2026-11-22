# Tasks: ASP.NET Core Package with Native ProblemDetails Interop (MVC)

**Input**: Design documents from `specs/006-aspnetcore-mvc-interop/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/public-api.md, contracts/problem-mapping.md

**Tests**: Required (constitution Principle III – TDD). Test tasks come first and must fail before the implementation that makes them pass.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: Setup

- [ ] T001 Record baseline: `dotnet build Viana.Results.slnx -c Release` and `dotnet test Viana.Results.slnx -c Release` (0 warnings, 336 tests passing)
- [ ] T002 Create src/Viana.Results.AspNetCore/Viana.Results.AspNetCore.csproj: `TargetFrameworks=net8.0;net10.0`, `FrameworkReference Microsoft.AspNetCore.App`, `ProjectReference ..\Viana.Results\Viana.Results.csproj`, packaging metadata copied from src/Viana.Results.Mvc/Viana.Results.Mvc.csproj (PackageId `Viana.Results.AspNetCore`, `Version` 1.0.0, `IsPackable=true`, `GenerateDocumentationFile=true`, README packed, release notes "Initial release: MVC integration with native ProblemDetails interop.")
- [ ] T003 Add `InternalsVisibleTo("Viana.Results.AspNetCore")` and `InternalsVisibleTo("Viana.Results.AspNetCore.Tests")` only if needed by the mapper/tests in src/Viana.Results/AssemblyInfo.cs (prefer public core APIs; skip if not needed)
- [ ] T004 Add central versions for `Microsoft.AspNetCore.Mvc.NewtonsoftJson` in Directory.Packages.props (8.0.x for net8.0, 10.0.x for net10.0 using TFM conditions on the PackageVersion items), no vulnerability advisories
- [ ] T005 Create tests/Viana.Results.AspNetCore.Tests/Viana.Results.AspNetCore.Tests.csproj (net8.0;net10.0, xunit, Microsoft.NET.Test.Sdk, coverlet.collector, Microsoft.AspNetCore.Mvc.Testing, Microsoft.AspNetCore.Mvc.NewtonsoftJson, ProjectReference to src/Viana.Results.AspNetCore) following tests/Viana.Results.Mvc.Tests/Viana.Results.Mvc.Tests.csproj
- [ ] T006 Add both projects to Viana.Results.slnx (`/src/` and `/tests/` folders); build: 0 warnings

## Phase 2: Foundational

- [ ] T007 Create test host helpers in tests/Viana.Results.AspNetCore.Tests/TestHost/: a builder that starts an in-memory ASP.NET Core app (TestServer) with configurable services (`AddProblemDetails` on/off, `CustomizeProblemDetails` callback with an invocation counter, `AddNewtonsoftJson`, `AddXmlSerializerFormatters`, `AddVianaResultFilter`) and a test controller whose actions return: Viana results (via `ToActionResult()` and directly), and native `Problem(...)` with given status/title/type/detail for parity comparison
- [ ] T008 Spike test (research R1 gate) in tests/Viana.Results.AspNetCore.Tests/ProblemPipelineSpikeTests.cs: writing a native `ProblemDetails` with a preset `traceId` extension and null `Type` through `IProblemDetailsService.TryWriteAsync` on net8.0 and net10.0 asserts (a) preset `traceId` kept, (b) `type` gets the native RFC 9110 link, (c) `CustomizeProblemDetails` runs after and can overwrite title, (d) customization invoked exactly once. Record the outcome in specs/006-aspnetcore-mvc-interop/research.md R1 ("Verified" or "Fallback used for all cases")
- [ ] T009 Write tests for the internal mapper in tests/Viana.Results.AspNetCore.Tests/ProblemDetailsMapperTests.cs per data-model.md: status taken from `result.Status` (not `ProblemResult.Status`); `Title` copied; `Type` "about:blank" → null, other values copied; `Extensions["detail"]`/`["instance"]` promoted to `Detail`/`Instance` and removed from extensions; other extensions (`description`, `errors`, custom) copied
- [ ] T010 Implement src/Viana.Results.AspNetCore/ProblemDetailsMapper.cs (`internal static ProblemDetails ToProblemDetails(IResult result)` using `result.Problem` and `result.Status`); make T009 pass

**Checkpoint**: Pipeline behavior verified; mapper ready.

## Phase 3: User Story 1 – Failures look like native problem details (P1) 🎯 MVP

**Goal**: A failed Viana result produces the same response as the framework's own `Problem(...)`, with Viana values winning and `CustomizeProblemDetails` running last.

**Independent Test**: In the test host with `AddProblemDetails` + a customization adding `"app":"x"`, an action returning `Results.NotFound()` via `ToActionResult()` responds 404, `application/problem+json`, with native type link, Viana title, `traceId` and `app`.

### Tests (write first, must fail)

- [ ] T011 [P] [US1] tests/Viana.Results.AspNetCore.Tests/ProblemParityTests.cs: theory over statuses 400, 401, 403, 404, 409, 422, 500 comparing JSON bodies of a Viana failure and native `Problem(statusCode, title, type, detail)` with equal values (ignore `traceId` value, compare member set and other values); run with and without `AddProblemDetails`
- [ ] T012 [P] [US1] tests/Viana.Results.AspNetCore.Tests/ProblemPrecedenceTests.cs: explicit title/type/detail/instance/extensions kept; Viana `traceId` extension wins over native; `CustomizeProblemDetails` applied, runs after Viana values (can overwrite title), invoked once; content type `application/problem+json`; problem with non-error status (200) keeps status 200 and still writes the problem body; HTTP status equals `result.Status`
- [ ] T013 [US1] Cover all result kinds failing: `Result`, `Result<T>`, `ListResult<T>`, `PagedResult<T>` with a problem produce the problem body (not data) in tests/Viana.Results.AspNetCore.Tests/ProblemPrecedenceTests.cs

### Implementation

- [ ] T014 [US1] Implement src/Viana.Results.AspNetCore/VianaActionResult.cs (`public sealed class VianaActionResult(IResult result) : IActionResult`, `public IResult Result`): when `Result.Problem != null` → map with ProblemDetailsMapper, set response status `Result.Status`, try `IProblemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext, ProblemDetails })`; if service missing or returns false → `ProblemDetailsFactory.CreateProblemDetails(...)` + copy Viana extensions on top + `ObjectResult` with `ContentTypes = { "application/problem+json" }` executed via `ExecuteResultAsync` (follow T008 outcome)
- [ ] T015 [US1] Implement src/Viana.Results.AspNetCore/ResultActionResultExtensions.cs: `ToActionResult(this IResult)` and `ToActionResult<TResult>(this Task<TResult>) where TResult : IResult`; XML docs; make T011–T013 pass

**Checkpoint**: US1 complete.

## Phase 4: User Story 2 – Successes behave like normal MVC results (P1)

**Goal**: Success payloads go through MVC content negotiation and formatters.

**Independent Test**: With `AddNewtonsoftJson()` configured, a `Result<T>` success is formatted by Newtonsoft (e.g. Newtonsoft-only attribute honored) with the result status.

- [ ] T016 [P] [US2] tests/Viana.Results.AspNetCore.Tests/SuccessFormattingTests.cs: default System.Text.Json (app `JsonOptions` naming honored); Newtonsoft (a `[Newtonsoft.Json.JsonProperty("renamed")]` property is renamed); XML when `Accept: application/xml` and XML formatters added; status 201 kept; 204/no-data → empty body; `ListResult<T>` → JSON array; `PagedResult<T>` → wrapper with paging metadata; null value written as formatter writes null
- [ ] T017 [US2] Extend VianaActionResult in src/Viana.Results.AspNetCore/VianaActionResult.cs: success → `ResultResponseBody.GetBody(Result)`; null → `StatusCodeResult(Result.Status)`; otherwise `ObjectResult(body) { StatusCode = Result.Status }` executed with the action context; make T016 pass

**Checkpoint**: US1 and US2 complete.

## Phase 5: User Story 3 – Return results directly from actions (P2)

**Goal**: Opt-in global filter.

**Independent Test**: With `AddVianaResultFilter()`, an action returning `Result<T>` (sync and `Task<Result<T>>`) yields the same response as `ToActionResult()`.

- [ ] T018 [P] [US3] tests/Viana.Results.AspNetCore.Tests/FilterTests.cs: direct return sync and async equals explicit conversion (status, content type, body) for success and failure; non-result return values (`Ok("x")`, plain object) unchanged; without the filter, explicit conversion still works
- [ ] T019 [US3] Implement src/Viana.Results.AspNetCore/Filters/VianaResultFilter.cs (`IResultFilter`: `ObjectResult { Value: IResult r }` → `VianaActionResult(r)`) and src/Viana.Results.AspNetCore/Filters/VianaResultFilterExtensions.cs (`AddVianaResultFilter(this IMvcBuilder)`); make T018 pass

**Checkpoint**: All stories complete.

## Phase 6: Polish & Cross-Cutting

- [ ] T020 [P] Write src/Viana.Results.AspNetCore/README.md: install, `ToActionResult()`, `AddVianaResultFilter()`, precedence (Viana → native defaults → `CustomizeProblemDetails`), status rule (`result.Status`), `about:blank` → native type link, works with/without `AddProblemDetails()`, formatters/Newtonsoft, migration from Viana.Results.Mvc (package + namespace change; `Headers`/`ContentType` virtuals not carried over)
- [ ] T021 [P] Update README.md (root): add Viana.Results.AspNetCore to the packages table (targets net8.0, net10.0), install section and documentation links
- [ ] T022 [P] Add benchmarks/Viana.Results.Benchmarks/AspNetCoreBenchmarks.cs (`[MemoryDiagnoser]`): execute `VianaActionResult` vs old `Viana.Results.Mvc.VianaResultAction` for success and failure on a `DefaultHttpContext` with a minimal service provider and `MemoryStream` body; add ProjectReferences to both packages in benchmarks/Viana.Results.Benchmarks/Viana.Results.Benchmarks.csproj (TargetFramework stays net10.0)
- [ ] T023 Switch the example app from Viana.Results.Mvc to Viana.Results.AspNetCore in examples/Viana.Results.Examples.Shared (package/project reference + namespaces only, verifying SC-005), plus `services.AddProblemDetails()` in the example hosts
- [ ] T024 [P] (FR-013, FR-017) Tests first in tests/Viana.Results.OpenApi.Tests/ProblemResultSchemaTests.cs: the document contains one shared base problem component with members `type`, `title`, `status`, `detail`, `instance` (none required, nullable) and no `traceId`; responses reference it via `$ref`; a response with extra members (e.g. `errors`, `detail` example) uses `allOf` with the base `$ref` plus its own members
- [ ] T025 [P] (FR-014, FR-015) Tests first in tests/Viana.Results.OpenApi.Tests/ProblemSourcePrecedenceTests.cs (new): for one status, endpoint `[ResponseExample]` > action `[ProblemResult]` > controller `[ProblemResult]` > global example, with schema and example from the same source; global example ignored when the endpoint declares the status; two endpoint examples → union of members, non-shared members optional; action beats controller (regression for the current order bug); success examples unchanged
- [ ] T026 [P] (FR-016) Tests first: documented problem responses have only `application/problem+json` content, both pipelines (native transformer and Swashbuckle filter)
- [ ] T027 Implement FR-013/FR-017 in src/Viana.Results.OpenApi/Schemas/ProblemResultSchema.cs (base component registration + `$ref`/`allOf`, component name `VianaProblemDetails` or as decided) and FR-016 (problem content type) in src/Viana.Results.OpenApi/Schemas/ProblemResultSchema.cs `ApplyTo`/`Build`; make T024 and T026 pass
- [ ] T028 Implement FR-014/FR-015 in src/Viana.Results.OpenApi/Processing/OpenApiOperationProcessor.cs (`GetProblemAttributes` order: action wins; source selection per status merging `ApplyProblemResultAttributes` and `ApplyResponseExamples`; build schema from `ProblemResult` examples via the existing `ProblemResultSchema(options, ProblemResult)` constructor) and src/Viana.Results.OpenApi/ResponseExampleResolver.cs; make T025 pass
- [ ] T029 Validate on the running example apps (Scalar and Swagger): `/ResultSamples/result-typed` 500 and 503 schema and example come from the same source, `application/problem+json`, shared `$ref`; update src/Viana.Results.OpenApi/README.md and src/Viana.Results.OpenApi.Swashbuckle/README.md (precedence table, content type, shared schema) and release notes of both packages
- [ ] T030 Run quickstart.md steps 1–6: build 0 warnings; all tests pass (old Mvc tests unchanged); example 404 shows native type link + traceId + `application/problem+json`; run `--filter '*AspNetCore*' --job short` and record results in benchmarks/Viana.Results.Benchmarks/baseline/aspnetcore-1.0.0.md; `git diff --stat -- src/Viana.Results.Mvc` empty

## Dependencies & Execution Order

- T001 → T002 → T003 → T004 → T005 → T006 → T007 → T008 → T009 → T010
- US1: (T011 ∥ T012) → T013 → T014 → T015
- US2: T016 → T017 (same file as T014, after US1)
- US3: T018 → T019 (needs T015)
- Polish: (T020 ∥ T021 ∥ T022) → T023 → (T024 ∥ T025 ∥ T026) → T027 → T028 → T029 → T030

## Parallel Example

```text
US1 tests: T011 (ProblemParityTests.cs) ∥ T012 + T013 (ProblemPrecedenceTests.cs, same file → T012 then T013)
Polish:    T020 (package README) ∥ T021 (root README) ∥ T022 (benchmark)
```

## Implementation Strategy

1. MVP: Phases 1–3 (failures with native interop via `ToActionResult()`).
2. US2: success formatting.
3. US3: global filter.
4. Polish: docs, benchmark, example migration, final validation.
