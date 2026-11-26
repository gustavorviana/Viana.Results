# Tasks: Bug-fix Sweep — Results, Mvc and OpenApi

**Input**: Design documents from `specs/007-bugfix-results-mvc-openapi/`

**Prerequisites**: plan.md, spec.md, research.md (R1–R17), data-model.md, contracts/behavior-changes.md

**Tests**: Required (constitution Principle III, spec FR-017). Every item gets a regression test that fails before its fix.

## Phase 1: Setup

- [X] T001 Record baseline: `dotnet build Viana.Results.slnx -c Release` (0 warnings) and `dotnet test Viana.Results.slnx -c Release` (351 tests passing)

## Phase 2: Foundational

- [X] T002 Add `[assembly: InternalsVisibleTo("Viana.Results.OpenApi.Swashbuckle")]` and `[assembly: InternalsVisibleTo("Viana.Results.OpenApi.Tests")]` to the OpenApi project (new file src/Viana.Results.OpenApi/AssemblyInfo.cs), needed by M1 internal equivalents; build stays at 0 warnings

## Phase 3: User Story 1 – Response bodies match the result (P1) 🎯 MVP

**Goal**: I1, I8, MV3, I9, I10 fixed in the core.

**Independent Test**: `ResultResponseBody.GetBody` returns null for `Result<User>(null)`, the struct value for `Result<MyStruct>`, the problem for a failure with status 204; empty `Extensions` cannot be cast to `Dictionary<string, object?>`; "errorCode" then "ErrorCode" yields one member.

### Tests (write first, must fail)

- [X] T003 [P] [US1] Create tests/Viana.Results.Tests/ResultResponseBodyTests.cs: (I1) `Result<User>` success with null value → `GetBody` returns null; value set → returns value; (MV3) `new Result(204, new ProblemResult(400, "x"))` → `GetBody` returns the problem; `new Result(204)` without problem → null; (I8) `Result<PointStruct>` (record struct with properties) → returns the struct value (unwrapped); `Result<int>`, `Result<DateTime>`, `Result<Guid>`, `Result<string>`, `Result<MyEnum>`, `Result<int?>` → return the result itself (wrapped, unchanged); `ListResult<T>` → items; `PagedResult<T>` → result (wrapped)
- [X] T004 [P] [US1] In tests/Viana.Results.Tests/ProblemResultTests.cs add (I9): `new ProblemResult(404, "x").Extensions` is not a `Dictionary<string, object?>` and `ProblemResult.WithDetail(400, "x", "d").Extensions` likewise; (I10) extensions `{"errorCode":"E1"}` merged with `{"ErrorCode":"E2"}` via `new ProblemResult(...)` with a case-sensitive input dictionary → single key "errorCode" with value "E2"; `Extensions.ContainsKey("ERRORCODE")` is true
- [X] T005 [P] [US1] In tests/Viana.Results.Tests/ProblemBuilderTests.cs (I10): `AddExtension("errorCode","E1").AddExtension("ErrorCode","E2")` → one member "errorCode" = "E2"; `AddExtensions` with mixed casing follows the same rule; in tests/Viana.Results.Tests/ProblemResultJsonConverterTests.cs: reading `{"status":400,"errorCode":"E1","ErrorCode":"E2"}` → one member "errorCode" = "E2"

### Implementation

- [X] T006 [US1] In src/Viana.Results/ResultResponseBody.cs: check `result.Problem != null` before the `Status == 204` short-circuit (MV3); for unwrappable non-scalar results return `resultData?.Data` without the `?? new object()` fallback (I1)
- [X] T007 [US1] In src/Viana.Results/ResultReflections.cs rewrite `IsScalarLike` per research R3: true only for `bool`, `char`, `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `string`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, `Uri`, `Version`, enums, types named `System.DateOnly`/`System.TimeOnly`, and `Nullable<T>` of those (I8)
- [X] T008 [US1] In src/Viana.Results/ProblemResult.cs `NormalizeExtensions`: build the copy with `StringComparer.OrdinalIgnoreCase` (indexer assignment: last value wins, first casing kept) (I10); when empty return `private static readonly IReadOnlyDictionary<string, object?> EmptyExtensions = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase))` (I9); also use `OrdinalIgnoreCase` in `ProblemResult.WithDetail`'s copy
- [X] T009 [US1] In src/Viana.Results/ProblemBuilder.cs and src/Viana.Results/ProblemResultJsonConverter.cs (`Read`): switch the extensions dictionaries to `StringComparer.OrdinalIgnoreCase` (I10); make T003–T005 pass; run all test projects and update tests that relied on the old behavior (`{}` body, wrapped structs, case-sensitive keys), noting each change in the commit message

**Checkpoint**: US1 complete.

## Phase 4: User Story 2 – Mvc follows RFC 9457 and is efficient (P1)

**Goal**: MV1, MV2, MV4 fixed in `Viana.Results.Mvc`.

**Independent Test**: executing a failed result sets content type `application/problem+json`; `AddVianaResultFilter()` twice → a single filter in `MvcOptions.Filters`.

### Tests and baseline (first)

- [X] T010 [P] [US2] In tests/Viana.Results.Mvc.Tests/VianaResultActionTests.cs: (MV1) change the 4 failure assertions (asserting `"application/json"` for problems) to `"application/problem+json"`; success responses still `"application/json"`; (MV2) a request whose `RequestAborted` token is already cancelled completes without throwing and writes no body; a subclass overriding `SerializeToJson` still has its output written; body bytes identical to before for success and failure
- [X] T011 [P] [US2] In tests/Viana.Results.Mvc.Tests/VianaResultFilterTests.cs (MV4): calling `AddVianaResultFilter()` twice on the same `IMvcBuilder` yields exactly one filter for `VianaResultFilter` in the resolved `MvcOptions.Filters` and one `VianaResultFilter` service registration
- [X] T012 [P] [US2] Create benchmarks/Viana.Results.Benchmarks/MvcBenchmarks.cs (`[MemoryDiagnoser]`): execute `VianaResultAction` for a success `Result<WeatherLike>` and a failure on a `DefaultHttpContext` with `MemoryStream` body and a minimal service provider; add a ProjectReference to src/Viana.Results.Mvc in benchmarks/Viana.Results.Benchmarks/Viana.Results.Benchmarks.csproj; run `--filter '*MvcBenchmarks*'` BEFORE T013 and record the table in specs/007-bugfix-results-mvc-openapi/research.md under R5 ("before")

### Implementation

- [X] T013 [US2] In src/Viana.Results.Mvc/VianaResultAction.cs: content type `application/problem+json` when `result.Problem != null`, otherwise `ContentType` (MV1); write with `JsonSerializer.SerializeAsync(response.Body, body, body.GetType(), options, context.HttpContext.RequestAborted)`, catching `OperationCanceledException` when the token is cancelled; use the legacy `SerializeToJson` + `WriteAsync` path only when a subclass overrides `SerializeToJson` (cache the check per type in a static `ConcurrentDictionary<Type, bool>`) (MV2)
- [X] T014 [US2] In src/Viana.Results.Mvc/VianaResultFilterExtensions.cs: `services.TryAddScoped<VianaResultFilter>()` and `services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, VianaResultFilterSetup>())` with new `internal sealed class VianaResultFilterSetup : IConfigureOptions<MvcOptions>` in src/Viana.Results.Mvc/Filters/VianaResultFilterSetup.cs (MV4); make T010–T011 pass
- [X] T015 [US2] Run `--filter '*MvcBenchmarks*'` AFTER T013–T014 and record the table in research.md R5 ("after"); allocations must be lower than "before" (SC-006)

**Checkpoint**: US1 and US2 complete.

## Phase 5: User Story 3 – OpenAPI documentation valid and accurate (P1)

**Goal**: B1, B2, B4, B5 fixed in OpenApi (both pipelines), and docs follow I8.

**Independent Test**: a `[ProblemResult(404)]` example is a JSON object with numeric `status` and title "Not Found"; a `ValueTask<Result>` action gets an empty body; an `[AllowAnonymous]` action under `[Authorize]` has no 401/403.

### Tests (write first, must fail)

- [X] T016 [P] [US3] In tests/Viana.Results.OpenApi.Tests/ProblemResultSchemaTests.cs: (B1) `Build([...]).Content[...].Example` has `GetValueKind() == JsonValueKind.Object`, `status` is a number, an int extension is a number, a bool extension is a boolean, a string with `"` round-trips through `JsonDocument.Parse`; (B2) default title for 404 is "Not Found", for 422 "Unprocessable Content", for 499 "Error"; response `Description` equals the title
- [X] T017 [P] [US3] Create tests/Viana.Results.OpenApi.Tests/OperationProcessorTests.cs: (B4) action returning `ValueTask<Result>` → content cleared like `Task<Result>`; `ValueTask<Result<int>>` with `[ProblemResult(404)]` → 404 response added; (B5) controller `[Authorize]` + action `[AllowAnonymous]` → no 401/403; controller `[Authorize]` only → 401/403; endpoint metadata containing an `IAuthorizeData` (minimal API `RequireAuthorization()`) → 401/403; metadata with `IAuthorizeData` + `IAllowAnonymous` → none
- [X] T018 [P] [US3] (FR-002 docs, I8) Implemented in tests/Viana.Results.Tests/ResultReflectionsTests.cs (both pipelines decide via ResultReflections, internal to the core): the native `UnwrapResultSchemaTransformer` and the Swashbuckle `UnwrapResultFilter` document `Result<PointStruct>` as `PointStruct` (unwrapped) and keep `Result<int>` as today

### Implementation

- [X] T019 [US3] In src/Viana.Results.OpenApi/Schemas/ProblemResultSchema.cs: build the example as a `JsonObject` using `JsonSerializer.SerializeToNode(field.ExampleValue, options)` per field (B1); default title via `HttpStatusInfo.TryGetReasonPhrase(status, out var t) ? t : "Error"` (B2)
- [X] T020 [US3] In src/Viana.Results.OpenApi/Processing/OpenApiOperationProcessor.cs: add internal `UnwrapTaskType(Type)` handling `Task<>` and `ValueTask<>` by `GetGenericTypeDefinition()` and use it in `ClearBodyForBareResult` and `ApplyProblemResultAttributes` (B4); add internal `ApplyAuthorizeResponses(OpenApiOperation, IEnumerable<object> endpointMetadata, MethodInfo?, JsonSerializerOptions)` that adds 401/403 when metadata (plus method/class attributes when there is no metadata) contains `IAuthorizeData` and no `IAllowAnonymous` (B5); the public overload delegates to it
- [X] T021 [US3] Pass `context.Description.ActionDescriptor.EndpointMetadata` from src/Viana.Results.OpenApi/Transformers/UnauthorizedAndForbiddenOperationTransformer.cs and `context.ApiDescription.ActionDescriptor.EndpointMetadata` from src/Viana.Results.OpenApi.Swashbuckle/Filters/UnauthorizedAndForbiddenOperationFilter.cs to the internal overload; make T016–T018 pass

**Checkpoint**: US1–US3 complete.

## Phase 6: User Story 4 – OpenAPI package safe to configure and evolve (P2)

**Goal**: M1, M2, M3, M5, M6.

**Independent Test**: two `AddVianaResultExamples` calls both apply; a provider with constructor DI is created; obsolete warnings on internal helpers; case-only duplicate fields documented once.

### Tests (write first, must fail)

- [X] T022 [P] [US4] In tests/Viana.Results.OpenApi.Tests/ResponseExampleResolverTests.cs (M3): provider with a constructor dependency registered in DI is created via `ActivatorUtilities`; missing dependency → `InvalidOperationException` whose message contains the provider type name; create tests/Viana.Results.OpenApi.Tests/VianaResultOptionsTests.cs (M2): two `AddVianaResultExamples` calls → `IOptions<VianaResultOptions>.Value.GlobalExamples` contains both registrations AND `GetRequiredService<VianaResultOptions>()` returns the same merged instance
- [X] T023 [P] [US4] In tests/Viana.Results.OpenApi.Tests/ProblemResultSchemaTests.cs (M5): adding fields "errorCode" (example "E1") then "ErrorCode" (example "E2") → schema has a single property "errorCode" and the example has `"errorCode":"E2"`; no exception; no naming policy applied (a camelCase `PropertyNamingPolicy` does not rename "TraceId")
- [X] T024 [P] [US4] Create tests/Viana.Results.OpenApi.Tests/Swashbuckle/UnwrapResultFilterTests.cs (M6); this requires adding a ProjectReference to src/Viana.Results.OpenApi.Swashbuckle in tests/Viana.Results.OpenApi.Tests/Viana.Results.OpenApi.Tests.csproj (brings `Swashbuckle.AspNetCore`, already version-pinned in Directory.Packages.props): a response media type with `Example` and `Examples` set keeps them after `UnwrapResultFilter.Apply` while `Schema` is replaced
- [X] T025 [P] [US4] (M1, revised 2026-11-26) Replace tests/Viana.Results.OpenApi.Tests/ObsoleteHelpersTests.cs with tests/Viana.Results.OpenApi.Tests/InternalHelpersTests.cs: `OpenApiOperationProcessor`, `OpenApiDocumentProcessor`, `OpenApiSchemaProcessor`, `ResponseExampleResolver`, `ResolvedExample`, `ProblemFieldSchema` and `ProblemResultSchema` are not public (`Type.IsPublic == false`); written first and failing

### Implementation

- [X] T026 [US4] (M2) src/Viana.Results.OpenApi/ServiceCollectionExtensions.cs: `services.Configure(configure)` and `services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<VianaResultOptions>>().Value)` (research R17); src/Viana.Results.OpenApi/Transformers/ResponseExampleOperationTransformer.cs and src/Viana.Results.OpenApi.Swashbuckle/Filters/ResponseExampleOperationFilter.cs read `IOptions<VianaResultOptions>` (falling back to a directly registered `VianaResultOptions`)
- [X] T027 [US4] (M3) src/Viana.Results.OpenApi/ResponseExampleResolver.cs `CreateProvider`: DI → `ActivatorUtilities.CreateInstance(services, providerType)` → `Activator.CreateInstance` when no services; wrap failures in `InvalidOperationException($"Could not create example provider '{providerType.FullName}': {inner.Message}", inner)`
- [X] T028 [US4] (M5) src/Viana.Results.OpenApi/Schemas/ProblemResultSchema.cs: keep fields in an `OrdinalIgnoreCase` structure where adding an existing name replaces type/description/example but keeps the first name's casing (same rule as I10); remove the `TryAdd` drop; no naming policy, warning or exception
- [X] T029 [US4] (M6) src/Viana.Results.OpenApi.Swashbuckle/Filters/UnwrapResultFilter.cs: set only `media.Schema` on existing `OpenApiMediaType` instances
- [X] T030 [US4] (M1, revised 2026-11-26) Make the helpers listed in T025 internal (files under src/Viana.Results.OpenApi/Processing/, src/Viana.Results.OpenApi/ResponseExampleResolver.cs, src/Viana.Results.OpenApi/Schemas/): `public` → `internal`, remove their `[Obsolete("Internal helper; not meant for direct use.")]`, keep the XML docs; the whole `ProblemResultSchema` becomes internal (without `AddField`/`Build`/`ApplyTo` its public surface is unusable); remove the `#pragma warning disable CS0618` lines that existed only for these helpers (keep the ones for `WithDescription`/`Description`); make T025 pass with the documents unchanged

**Checkpoint**: All stories complete.

## Phase 7: Polish

- [X] T031a [P] (M1, revised 2026-11-26) src/Viana.Results.OpenApi/Viana.Results.OpenApi.csproj: `<Version>2.0.0</Version>`; replace the "Deprecated: … are internal helpers …" line with "Breaking: the helpers OpenApiOperationProcessor, OpenApiDocumentProcessor, OpenApiSchemaProcessor, ResponseExampleResolver, ResolvedExample, ProblemFieldSchema and ProblemResultSchema, public in 1.0.0, are now internal; use the transformers, attributes and extensions." and "Breaking: OpenApiOperationProcessor.ClearBodyForBareResult was removed."; check the Swashbuckle reference stays compatible
- [X] T031 [P] Release notes: src/Viana.Results/Viana.Results.csproj (append to 3.0.0: Fixed I1 null body, I8 struct unwrapping, MV3 problem with 204, I9 immutable empty extensions; Changed I10 case-insensitive extension names, last value wins); src/Viana.Results.Mvc/Viana.Results.Mvc.csproj `<Version>2.1.0</Version>` (Fixed: problem responses use application/problem+json; streamed writing honoring request abortion; idempotent AddVianaResultFilter); src/Viana.Results.OpenApi/Viana.Results.OpenApi.csproj and src/Viana.Results.OpenApi.Swashbuckle/Viana.Results.OpenApi.Swashbuckle.csproj (append B1, B2, B4, B5, M2, M3, M5, M6 and the M1 deprecation)
- [X] T032 [P] READMEs: src/Viana.Results/README.md (body rules: null → null, struct unwrapped; extension names case-insensitive), src/Viana.Results.Mvc/README.md (problem content type), src/Viana.Results.OpenApi/README.md and src/Viana.Results.OpenApi.Swashbuckle/README.md (AllowAnonymous / RequireAuthorization, ValueTask, example providers with DI)
- [X] T033 Run quickstart.md steps 1–6 (including running both example apps and checking the OpenAPI documents)

## Dependencies & Execution Order

- T001 → T002 → US1 ((T003 ∥ T004 ∥ T005) → T006 → T007 → T008 → T009) → US2 ((T010 ∥ T011 ∥ T012) → T013 → T014 → T015) → US3 ((T016 ∥ T017 ∥ T018) → T019 → T020 → T021) → US4 ((T022 ∥ T023 ∥ T024 ∥ T025) → T026 → T027 → T028 → T029 → T030) → (T031 ∥ T032) → T033
- US2 and US3 depend on US1 (Mvc uses `ResultResponseBody`; OpenAPI uses `IsScalarLike` and the extension rule); US4 builds on US3 files.

## Parallel Example

```text
US1 tests: T003 (ResultResponseBodyTests.cs) ∥ T004 (ProblemResultTests.cs) ∥ T005 (ProblemBuilderTests.cs + converter tests)
US2:       T010 ∥ T011 ∥ T012 (benchmark baseline)
US3 tests: T016 ∥ T017 ∥ T018
US4 tests: T022 ∥ T023 ∥ T024 ∥ T025
Polish:    T031 ∥ T032
```

## Implementation Strategy

1. MVP: US1 (core body and extension fixes, they also flow into Mvc and OpenAPI).
2. US2: Mvc RFC content type, streaming, idempotent filter (benchmark before/after).
3. US3: OpenAPI correctness.
4. US4: OpenAPI robustness and deprecations.
5. Polish: notes, READMEs, end-to-end validation.
