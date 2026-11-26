# Tasks: Envelope results

**Input**: [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [data-model.md](data-model.md),
[contracts/public-api.md](contracts/public-api.md), [quickstart.md](quickstart.md)

**Tests**: required. The constitution (III) mandates test-first: each story's tests are written first and must
fail. Tests and examples use one type per file.

## Phase 1: Setup

- [X] T001 Record the baseline benchmark before any change. Reuse `benchmarks/Viana.Results.Benchmarks/MvcBenchmarks.cs`
      and `AspNetCoreBenchmarks.cs`, and add `PagedResult<T>` and `Result<T>` write cases there if missing. Save the
      output under `benchmarks/Viana.Results.Benchmarks/baseline/envelope-before.md`.

## Phase 2: Foundational (blocks all stories)

- [X] T002 [P] Write failing core tests in `tests/Viana.Results.Tests/Envelopes/EnvelopeBodyTests.cs`. Put the
      test types in their own files: `CursorPage.cs`, `TestCursorResult.cs` (implements
      `IEnvelopeResult<IReadOnlyList<CursorItem>>` directly, not derived from `Result`), `CursorItem.cs` and
      `RenamedCursorResult.cs` (`[JsonPropertyName("items")]` on `Data`). Cover these cases:
      - `ResultReflections.IsEnvelopeType` is true for `TestCursorResult` and `PagedResult<T>`, and false for
        `Result<T>` and `ListResult<T>`.
      - `IsUnwrappableType` is false for envelopes.
      - `GetBody(result, options)` gives a `JsonObject` with `data` and `nextCursor`, without `status` or
        `problem`, and with camelCase naming from the options.
      - `[JsonPropertyName("items")]` gives `items`.
      - A `[JsonIgnore]` property is not written.
      - A null `Data` is written as null.
      - With a problem, the body is the problem. With status 204, the body is null.
      - For non-envelopes, `GetBody(result, options)` equals `GetBody(result)`, unchanged.
- [X] T003 Add the public `IEnvelopeResult<out T> : IResult<T>` with XML docs in
      `src/Viana.Results/IEnvelopeResult.cs`.
- [X] T004 Add `ResultReflections.IsEnvelopeType(Type)`, and make `IsUnwrappableType` return false for envelopes,
      in `src/Viana.Results/ResultReflections.cs`. Keep the `IPagedResult<>` check until T015.
- [X] T005 Implement the internal `EnvelopeBody.Build(IResult, JsonSerializerOptions)` in
      `src/Viana.Results/EnvelopeBody.cs`, following R2:
      - serialize with `options.GetTypeInfo(type)` to a `JsonObject`;
      - remove the contract properties backed by `Status`, `Problem` and `Extensions`, matched by CLR property,
        not by JSON name;
      - use no `RequiresUnreferencedCode` APIs.

      Add `ResultResponseBody.GetBody(IResult?, JsonSerializerOptions)` in `src/Viana.Results/ResultResponseBody.cs`
      with the precedence from data-model.md: problem, then 204, then envelope, then today's rules. Make T002 pass.

**Checkpoint**: the full suite is green. Non-envelope output is unchanged.

## Phase 3: User Story 1, custom envelope at runtime (P1) 🎯 MVP

**Independent test**: a `CursorResult` returned from Mvc, AspNetCore controllers and AspNetCore minimal APIs
gives `{ "data": [...], "nextCursor": "abc" }`.

- [X] T006 [P] [US1] Write failing runtime tests in
      `tests/Viana.Results.Mvc.Tests/Envelopes/EnvelopeWriteTests.cs`, with a `MvcCursorResult.cs` type. Cover
      `VianaResultAction` and the global filter, with a body, a problem (`application/problem+json`) and a 204.
- [X] T007 [P] [US1] Write failing runtime tests in
      `tests/Viana.Results.AspNetCore.Tests/Envelopes/EnvelopeWriteTests.cs`, with an `AspNetCursorResult.cs`
      type. Cover `ToActionResult()`, the filter, `ToHttpResult()` and `AddVianaResults()` handlers on net8.0 and
      net10.0. Include `[JsonPropertyName]` and a naming-policy case.
- [X] T008 [US1] Pass the serializer options to `GetBody`:
      - in `src/Viana.Results.Mvc/VianaResultAction.cs`, the options it already uses;
      - in `src/Viana.Results.AspNetCore/VianaActionResult.cs`, `IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>`
        from the request services;
      - in `src/Viana.Results.AspNetCore/VianaHttpResult.cs`, `IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>`.

      In `VianaActionResult`, use the `JsonObject` only when `MvcOptions.OutputFormatters` contains a
      `SystemTextJsonOutputFormatter`; otherwise keep the instance itself (R3). Add a test with Newtonsoft-only
      formatters in T007. Make T006 and
      T007 pass.

## Phase 4: User Story 4, OpenAPI matches the runtime (P1)

**Independent test**: on both pipelines, the schema members of a cursor endpoint equal the runtime body members.

- [X] T009 [P] [US4] Write failing tests in `tests/Viana.Results.OpenApi.Tests/Envelopes/EnvelopeSchemaTests.cs`,
      with `DocCursorResult.cs`, `DocCursorController.cs`, `DocCursorExample.cs` and `ExtendedCursorResult.cs`.
      Cover a controller and a minimal API on native and Swashbuckle:
      - the schema has `data` and `nextCursor`, and no `status`, `problem` or `extensions`;
      - `[JsonPropertyName]` is reflected;
      - with `IHasExtensions`, `additionalProperties` is allowed;
      - a `[ResponseExample]` or `ProducesResult` provider returning an envelope gives the same JSON as the runtime
        `GetBody(result, options)`.
- [X] T010 [US4] In the schema processor used by `OmitProblemSchemaTransformer` and `OmitProblemSchemaFilter`
      (`src/Viana.Results.OpenApi/Processing/`), also remove `extensions` and set `additionalProperties` for
      `IHasExtensions` envelopes.
- [X] T011 [US4] In `src/Viana.Results.OpenApi/Processing/OperationProcessor.cs`, build examples with
      `GetBody(result, json)` (both places that call `GetBody` today). Make T009 pass.

## Phase 5: User Story 2, extensions at the root (P2)

**Independent test**: `Extensions = { region: "eu", data: "x" }` gives `region` at the root and keeps the declared
`data`.

- [X] T012 [P] [US2] Write failing tests in `tests/Viana.Results.Tests/Envelopes/EnvelopeExtensionsTests.cs`, with
      `ExtendedTestCursorResult.cs`:
      - entries are written at the root, and no `extensions` member appears;
      - a declared property wins on a case-insensitive clash;
      - null values are written as null;
      - values are serialized with their runtime type.

      Add one runtime check in the AspNetCore tests from T007.
- [X] T013 [US2] Extend `EnvelopeBody.Build` in `src/Viana.Results/EnvelopeBody.cs` to flatten the
      `IHasExtensions.Extensions` entries. Serialize each value with `options.GetTypeInfo(value.GetType())`. Make
      T012 pass.

## Phase 6: User Story 3, PagedResult is an envelope (P2)

**Independent test**: the existing paged tests in every package pass without changes.

- [X] T014 [P] [US3] Add a test that checks `PagedResult<T>` is an `IEnvelopeResult<IReadOnlyList<T>>` and that its
      body is exactly `{ "data": [...], "pageNumber": n, "totalPages": m }`, in
      `tests/Viana.Results.Tests/Envelopes/PagedEnvelopeTests.cs`.
- [X] T015 [US3] Make `PagedResult<T>` implement `IEnvelopeResult<IReadOnlyList<T>>` (`src/Viana.Results/PagedResult.cs`)
      and remove the `IPagedResult<>` special case from `IsUnwrappableType` (`src/Viana.Results/ResultReflections.cs`).
      All existing paged tests must stay green without edits (SC-003).

## Phase 7: Polish

- [X] T016 [P] Add the example (FR-010):
      - `examples/Viana.Results.Examples.Shared/Catalog/CursorResult.cs` (`IEnvelopeResult<IReadOnlyList<T>>` with
        `Data` and `NextCursor`);
      - a cursor endpoint in the shared controllers and in the minimal endpoints;
      - an example provider;
      - coverage in `tests/Viana.Results.Examples.Tests`.

      One type per file.
- [X] T017 [P] Add release notes in `src/Viana.Results/Viana.Results.csproj`, the Mvc, AspNetCore and OpenApi
      csproj files and the Swashbuckle csproj (escape `<`/`>`), with no version bump. In
      `src/Viana.Results/README.md`, add at most a short `CursorResult<T>` snippet. The full rules go to the wiki
      list. Do not name any other mediator library.
- [X] T018 Run the benchmark again. Save `benchmarks/Viana.Results.Benchmarks/baseline/envelope-after.md` and
      compare it with T001. Justify any `PagedResult` regression in this file under "Benchmark".
- [X] T019 Build `src/Viana.Results` with `-p:IsAotCompatible=true` and confirm that no trim or AOT warning comes
      from the new code. Record the result under "AOT check" below.
- [X] T020 Run [quickstart.md](quickstart.md): Release build with 0 warnings, all tests on every TFM, and the Scalar
      example cursor endpoint (its body and its schema match). Set the spec status to Ready.

## Benchmark

The before and after results are in `benchmarks/Viana.Results.Benchmarks/baseline/envelope-before.md` and
`envelope-after.md`. Both use the ShortRun job on net10.0.

The first implementation regressed:

- `PagedResult` writes went from about 3 µs to 7–11 µs, and their allocation from about 3.4 KB to about 6 KB,
  because every envelope was rebuilt as a `JsonObject`.
- Plain successes allocated about 0.1 KB more, because `GetInterfaces()` ran on every call.

The regression was fixed before closing the task:

- `ResultReflections.IsEnvelopeType` is cached per type.
- `EnvelopeBody.Shape` builds a node only when the contract actually writes `Status`/`Problem`/`Extensions`, or
  when the type has extensions. The decision is made once per `JsonTypeInfo`. Otherwise the instance itself is the
  body, which covers `PagedResult` because it inherits the `[JsonIgnore]` members of `Result`.
- `VianaActionResult` checks the output formatters with an indexed loop, and only for envelopes.

Final allocation, before → after:

| Case | Before | After |
|---|---|---|
| MvcSuccess | 2.67 KB | 2.67 KB |
| AspNetCoreSuccess | 3.22 KB | 3.22 KB |
| MinimalSuccess | 3.00 KB | 3.00 KB |
| MvcPaged | 3.39 KB | 3.31 KB |
| AspNetCorePaged | 3.94 KB | 3.86 KB |
| MinimalPaged | 3.72 KB | 3.64 KB |

The failure cases are unchanged. Mean times are within the ShortRun error margins, and the paged cases are about
3 µs, as before. There is no remaining regression to justify. Envelopes that do need shaping, such as a
`CursorResult` that does not derive from `Result`, pay one `SerializeToNode` by design.

Deviation: T016 asked for a minimal endpoint as well. It was added (`/minimal/products/cursor`), and both routes
are covered in `SamplesRoutesTests`.

## AOT check

`src/Viana.Results` was built for net10.0 with `-p:IsAotCompatible=true`, before and after 014.

- **Before** (the 014 source files stashed): 6 warnings, all pre-existing. Four are IL2026/IL3050 in
  `ProblemResultJsonConverter` (lines 126 and 158), and two are IL2070 in `ResultReflections.IsUnwrappableType` /
  `GetDataType`.
- **After**: the same 6 warnings, and no new one.
  - `EnvelopeBody` uses only `JsonTypeInfo` APIs.
  - Its reflection-resolver fallback for options without a resolver only runs when
    `JsonSerializer.IsReflectionEnabledByDefault`. It carries a narrow, justified `UnconditionalSuppressMessage`
    (IL2026/IL3050).
  - `IsEnvelopeType` carries a narrow, justified IL2070 suppression. Annotating its parameter instead only moved
    the warning to the callers.
- The pre-existing warnings and the missing CI AOT smoke test are out of scope (SC-004).

## Dependencies

- T001 → T002–T005 → US1 (T006–T008) and US4 (T009–T011) → US2 (T012–T013) → US3 (T014–T015) → Polish.
- US2 changes `EnvelopeBody.cs`, which US1 and US4 rely on. Its tests check the runtime and the documents again.
- US3 is last because it removes the special case once envelopes work end to end.

## Parallel examples

- Phase 2: T002 ∥ T003.
- US1: T006 ∥ T007. US4: T009 ∥ T006/T007.
- Polish: T016 ∥ T017.

## Implementation strategy

The MVP is Phase 2 plus US1: a user can return a cursor envelope. Then US4 makes the documents match, US2 adds the
extensions, and US3 brings `PagedResult` into the rule. Commit docs, examples and code separately.
