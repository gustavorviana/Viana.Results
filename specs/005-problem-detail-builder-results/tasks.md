# Tasks: RFC 9457 "detail" and Builder Results

**Input**: Design documents from `specs/005-problem-detail-builder-results/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/public-api.md

**Tests**: Required (constitution Principle III – TDD). Tests first; they must fail before the implementation.

## Phase 1: Setup

- [X] T001 Record baseline: `dotnet build Viana.Results.slnx -c Release` (0 warnings) and `dotnet test Viana.Results.slnx -c Release` (336 tests passing)

## Phase 2: Foundational

- [X] T002 In src/Viana.Results/ProblemResult.cs add `internal static ProblemResult CreateWithDescription(int status, string title, string? description, string type, IReadOnlyDictionary<string, object?>? extensions)` with the current `WithDescription` body; make `WithDescription` delegate to it; switch `ProblemBuilder.Build()` (src/Viana.Results/ProblemBuilder.cs) and `ProblemResultJsonConverter.Read` (src/Viana.Results/ProblemResultJsonConverter.cs, also update its comment) to the internal factory. No behavior change; all tests still pass

## Phase 3: User Story 1 – Use the standard "detail" member (P1) 🎯 MVP

**Goal**: `ProblemResult.WithDetail` exists; `description` APIs are obsolete with the 4.0.0 notice.

**Independent Test**: `ProblemResult.WithDetail(400, "Bad Request", "Email is invalid.")` stores `Extensions["detail"]`; a call to `WithDescription` compiles with CS0618 naming `WithDetail` and 4.0.0.

### Tests (write first, must fail)

- [X] T003 [P] [US1] In tests/Viana.Results.Tests/ProblemResultTests.cs add: `WithDetail` stores `Extensions["detail"]` and no `description`; keeps other extensions; null/"" detail not stored; blank title still defaults to reason phrase; non-empty detail overwrites an `extensions["detail"]` passed in
- [X] T004 [P] [US1] Create tests/Viana.Results.Tests/ObsoleteApiTests.cs asserting via reflection that `ProblemResult.WithDescription` and `ProblemBuilder.WithDescription` have `ObsoleteAttribute` whose `Message` contains `"RFC 9457"`, `"4.0.0"` and the replacement name (`ProblemResult.WithDetail` / `ProblemBuilder.WithDetail`), and `IsError == false`

### Implementation

- [X] T005 [US1] In src/Viana.Results/ProblemResult.cs add `public static ProblemResult WithDetail(int status, string title, string? detail = null, string type = "about:blank", IReadOnlyDictionary<string, object?>? extensions = null)` with XML docs (RFC 9457 §3.1.4); add `[Obsolete("\"description\" is not an RFC 9457 member and will be removed in Viana.Results 4.0.0. Use ProblemResult.WithDetail (RFC 9457 \"detail\") instead.")]` to `WithDescription`; update XML docs of `Extensions` to mention `detail`
- [X] T006 [US1] In src/Viana.Results/ProblemBuilder.cs add `[Obsolete("\"description\" is not an RFC 9457 member and will be removed in Viana.Results 4.0.0. Use ProblemBuilder.WithDetail (RFC 9457 \"detail\") instead.")]` to `WithDescription`
- [X] T007 [US1] Wrap every remaining call to the obsolete APIs in tests with `#pragma warning disable CS0618 // exercises the obsolete "description" API until it is removed in 4.0.0` / `#pragma warning restore CS0618` around the single statement: tests/Viana.Results.Tests/ProblemResultTests.cs, tests/Viana.Results.Tests/ProblemBuilderTests.cs, tests/Viana.Results.Tests/ProblemResultJsonConverterTests.cs, tests/Viana.Results.Mvc.Tests/VianaResultActionTests.cs (lines using `WithDescription`); where a test only uses `WithDescription(..., null, ...)` to pass extensions, switch it to `new ProblemResult(...)` instead of suppressing
- [X] T008 [US1] Migrate examples/Viana.Results.Examples.Shared/Examples/InternalServerErrorExample.cs and ServiceUnavailableExample.cs from `ProblemResult.WithDescription(..., description: ...)` to `ProblemResult.WithDetail(..., detail: ...)`; build: 0 warnings; T003–T004 pass

**Checkpoint**: US1 complete.

## Phase 4: User Story 2 – Build a result with the problem inside (P1)

**Goal**: `BuildResult()` / `BuildResult<T>()` on the builder.

**Independent Test**: `new ProblemBuilder(404).WithDetail("x").BuildResult()` → failure, status 404, `Problem.Extensions["detail"] == "x"`.

- [X] T009 [US2] In tests/Viana.Results.Tests/ProblemBuilderTests.cs add: `BuildResult()` returns failure (`Problem` not null) with builder status and problem members equal to `Build()`; `BuildResult<int>()` returns `Result<int>` failure with same status/problem and no value; title default (reason phrase) applies; detail/type/extensions preserved; a builder with both `WithDescription` (suppressed CS0618) and `WithDetail` keeps both `description` and `detail` (spec edge case)
- [X] T010 [US2] In src/Viana.Results/ProblemBuilder.cs add `public Result BuildResult() => new Result(Build());` and `public Result<T> BuildResult<T>() => new Result<T>(Build());` with XML docs; make T009 pass

**Checkpoint**: US1 and US2 complete.



## Phase 5: User Story 3 – Document "detail" in OpenAPI problem responses (P2)

**Goal**: `ProblemResultAttribute.Detail` appears in the documented schema; `Description` is obsolete.

**Independent Test**: `ProblemResultSchema.FromAttribute(options, new ProblemResultAttribute(400) { Detail = "One or more fields are invalid." })` applied to an operation produces a 400 problem schema with a `detail` string property whose example is the text.

### Tests (write first, must fail)

- [X] T011 [P] [US3] In tests/Viana.Results.OpenApi.Tests/ProblemResultAttributeTests.cs: `Detail` defaults to null and stores the value; wrap the existing statements using `Description` with `#pragma warning disable CS0618 // exercises the obsolete Description option until it is removed in 2.0.0` / restore; assert via reflection that `Description` has `ObsoleteAttribute` whose message contains `"no effect"`, `"2.0.0"` and `"Detail"`
- [X] T012 [P] [US3] In tests/Viana.Results.OpenApi.Tests/ProblemResultSchemaTests.cs: `FromAttribute` with `Detail` adds a `detail` property (type string, example = text) to the problem schema; without `Detail` the schema has the same properties as today (status, type, title only)

### Implementation

- [X] T013 [US3] In src/Viana.Results.OpenApi/ProblemResultAttribute.cs add `public string? Detail { get; set; }` (XML doc: RFC 9457 "detail", documented in the problem schema) and `[Obsolete("Description has no effect and will be removed in Viana.Results.OpenApi 2.0.0. Use Detail (RFC 9457 \"detail\") instead.")]` on `Description`; update the class remarks example to use `Detail`
- [X] T014 [US3] In src/Viana.Results.OpenApi/Schemas/ProblemResultSchema.cs `FromAttribute`: when `attribute.Detail` is not null/empty, `AddField(JsonSchemaType.String, "detail", "RFC 9457: human-readable explanation specific to this occurrence", attribute.Detail)`; make T011–T012 pass; confirm the Swashbuckle path (`ProblemResponseOperationFilter` → `ApplyProblemResultAttributes`) picks it up with a test in tests/Viana.Results.OpenApi.Tests
- [X] T015 [US3] Use `Detail` in the example app (add `Detail = "..."` to one `[ProblemResult]` in examples/Viana.Results.Examples.Shared/Controllers/ResultSamplesController.cs); update src/Viana.Results.OpenApi/README.md and src/Viana.Results.OpenApi.Swashbuckle/README.md (document `Detail`, deprecation of `Description`)
- [X] T016 [US3] Bump src/Viana.Results.OpenApi/Viana.Results.OpenApi.csproj to 1.1.0 (release notes: Added `ProblemResultAttribute.Detail`; Deprecated `Description`, no effect, removed in 2.0.0) and src/Viana.Results.OpenApi.Swashbuckle/Viana.Results.OpenApi.Swashbuckle.csproj to 1.1.2 (release notes: `Detail` from Viana.Results.OpenApi 1.1.0 is documented in Swashbuckle output)

**Checkpoint**: US1–US3 complete.

## Phase 6: Polish

- [X] T017 [P] Update src/Viana.Results/README.md: replace `WithDescription` usage with `WithDetail`; document `ProblemResult.WithDetail`, `BuildResult()`/`BuildResult<T>()`; add "Deprecated" note (`description` → `detail`, removal in 4.0.0, link RFC 9457 §3.1.4)
- [X] T018 [P] Add `BuilderBuildResult` benchmark (`new ProblemBuilder(404).WithDetail("x").BuildResult()`) to benchmarks/Viana.Results.Benchmarks/ProblemBenchmarks.cs and update the catalog table in benchmarks/README.md
- [X] T019 Set `<Version>3.0.0</Version>` in src/Viana.Results/Viana.Results.csproj and rewrite `PackageReleaseNotes` starting with: "Major release. No public API removed; deprecations announced for 4.0.0. Behavior change: default title (feature 002)." then: Changed: a problem without title defaults to the status reason phrase; Added: `ProblemResult.WithDetail`, `ProblemBuilder.BuildResult()`/`BuildResult<T>()`; Deprecated: `ProblemResult.WithDescription`, `ProblemBuilder.WithDescription` (removed in 4.0.0, use `WithDetail`)
- [X] T020 Update specs/002-problem-title-reason-phrase/spec.md FR-006 note: shipped in 3.0.0 together with feature 005 (owner decision 2026-11-21)
- [X] T021 Run quickstart.md steps 1–9 (build 0 warnings, all tests, grep checks, benchmarks vs baseline-2.1.0.md and record `BuilderBuildResult`, OpenAPI document check, versions); step 5 is optional (T004 is the official obsolete-message check)

## Dependencies & Execution Order

- T001 → T002 → (T003 ∥ T004) → T005 → T006 → T007 → T008 → T009 → T010 → (T011 ∥ T012) → T013 → T014 → T015 → T016 → (T017 ∥ T018) → T019 → T020 → T021
- US2 is independent of US1 functionally but shares ProblemBuilder.cs, so it runs after T006.
- US3 only touches OpenAPI files and could run in parallel with US2.

## Parallel Example

```text
US1 tests: T003 (ProblemResultTests.cs) ∥ T004 (ObsoleteApiTests.cs)
US3 tests: T011 (ProblemResultAttributeTests.cs) ∥ T012 (ProblemResultSchemaTests.cs)
Polish:    T017 (core README) ∥ T018 (benchmark)
```

## Implementation Strategy

1. MVP: US1 (detail + deprecations).
2. US2: builder results.
3. US3: OpenAPI `Detail` + deprecation.
4. Polish: docs, benchmark, version 3.0.0, validation.
5. Then feature 006 (ASP.NET Core) maps only `detail`.
