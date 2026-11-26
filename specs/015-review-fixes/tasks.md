# Tasks: Review fixes

**Input**: [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [quickstart.md](quickstart.md)

**Tests**: required. The constitution (III) mandates test-first: tests are written first and must fail. Tests use
one type per file.

## Phase 1: Setup

No setup is needed. Every project and baseline already exists, and the 014 `envelope-after.md` is the performance
reference.

## Phase 2: Foundational

None. The three stories are independent.

## Phase 3: User Story 1, contract-based envelope shaping (P1) 🎯 MVP

**Independent test**: an envelope whose `Status` is ignored when default never writes `status`, even when the
first instance written has status 0.

- [X] T001 [P] [US1] Write failing tests in `tests/Viana.Results.Tests/Envelopes/ConditionalEnvelopeTests.cs`,
      with a `ConditionalStatusCursorResult.cs` type: `Status` has
      `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]`, and `Problem` has
      `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`. Cover these cases:
      - with fresh options, write an instance with status 0 (so `Status` is skipped by its condition), then one
        with status 200 and a null `Problem` (so `Status` is written by the contract and `Problem` is skipped);
        neither body contains `status` or `problem`;
      - the same instances in reverse order give the same result;
      - `PagedResult` is still returned as the instance itself (`Assert.Same`).
- [X] T002 [US1] In `src/Viana.Results/EnvelopeBody.cs`, replace `ComputeNeedsShaping` with the contract rule from
      R1. A container member is "never written" when `JsonPropertyInfo.Get == null` or its CLR property has
      `[JsonIgnore]` with `Condition == JsonIgnoreCondition.Always`. Otherwise it "may be written". Any
      `IHasExtensions` type always needs shaping. Keep the per-`JsonTypeInfo` cache, and stop serializing an
      instance to decide. Make T001 pass, and keep the existing 014 envelope tests green.

## Phase 4: User Story 2, responses on empty operations (P2)

**Independent test**: an authorized endpoint with null `Responses` documents 401/403, and a problem
`[ResponseExample]` on an operation with empty `Responses` documents the problem.

- [X] T003 [P] [US2] Write failing tests in `tests/Viana.Results.OpenApi.Tests/EmptyResponses/EmptyResponsesTests.cs`,
      with `SecuredController.cs` (`[Authorize]` plus a `Result<T>` action) and `ProblemOnlyController.cs` (a
      `Result<T>` action with a problem `[ResponseExample]` and a success `[ResponseExample]`). Cover these cases:
      - `ApplyAuthorizeResponses` on `new OpenApiOperation()` (null `Responses`) adds 401 and 403;
      - `ApplyResponseExamples` on an operation with empty `Responses` adds the problem response with its example;
      - the success example is still not added (US2 scenario 3).

      Reuse a problem example type from the test project, or add one in its own file.
- [X] T004 [US2] In `src/Viana.Results.OpenApi/Processing/OperationProcessor.cs`, make these changes:
      - In `ApplyAuthorizeResponses`, add `operation.Responses ??= new OpenApiResponses();` after the
        `requiresAuthorization` check.
      - In `ApplyResponseExamples`, replace `if (operation.Responses == null || operation.Responses.Count == 0) return;`
        with `operation.Responses ??= new OpenApiResponses();`. The non-problem path keeps skipping statuses
        without a response.

      Make T003 pass.

## Phase 5: User Story 3, no future-version announcements (P3)

**Independent test**: the R4 search outside `specs/` finds nothing.

- [X] T005 [US3] Run the R4 search over `src/`, `tests/`, `examples/`, `README.md`, the package READMEs and the
      csproj release notes. List every hit in this file under "Version search". Review each hit by hand: text that
      describes a change already made (for example "Changed: …" in release notes) stays, and the list records why.
      Only announcements of a future version are rewritten.
- [X] T006 [US3] Rewrite each hit:
      - `[Obsolete]` messages and XML docs name the replacement only;
      - release notes drop the planned version;
      - test comments and `#pragma` comments drop the version.

      Rerun the search and confirm zero hits.

## Phase 6: Polish

- [X] T007 [P] Add release notes (no version bump):
      - `src/Viana.Results/Viana.Results.csproj`: "Fixed: envelope shaping is decided from the type contract, so
        conditionally written Status/Problem never reach the body";
      - `src/Viana.Results.OpenApi/Viana.Results.OpenApi.csproj` and the Swashbuckle csproj: "Fixed: 401/403 and
        problem examples are documented on operations without responses".

      Escape `<`/`>`.
- [X] T008 Rerun the benchmark (`--filter *AspNetCoreBenchmarks* *MvcBenchmarks* --job short`) and compare the
      allocation with `benchmarks/Viana.Results.Benchmarks/baseline/envelope-after.md`. It must be the same or
      lower. Record the result under "Benchmark".
- [X] T009 Build the core with `-p:IsAotCompatible=true`. Confirm the same 6 pre-existing warnings and no new one,
      and record the result under "AOT check".
- [X] T010 Run [quickstart.md](quickstart.md): Release build with 0 warnings and all tests on every TFM. Set the
      spec status to Ready.

## Version search

23 hits before the rewrite, all announcements of a future version. None was legitimate change text, so all
were rewritten:

- 3 `[Obsolete]` messages: `ProblemResult.WithDescription`, `ProblemBuilder.WithDescription` and
  `ProblemResultAttribute.Description`. They now name the replacement only.
- 11 more `[Obsolete]` messages on the OpenApi internal helpers, which said "will become internal in 2.0.0". They
  now read "Internal helper; not meant for direct use."
- 3 release notes, in the core and OpenApi csproj files.
- 17 `#pragma warning disable CS0618` comments in the tests, which no longer say "until it is removed in X".

Three obsolete-message tests asserted the version text (`ObsoleteApiTests`, `ProblemResultAttributeTests` and
`ObsoleteHelpersTests`). They now assert that the message names the replacement and contains no version. A second
search finds 0 hits outside `specs/`.

## Benchmark

The first run after the US1 change regressed `AspNetCorePaged` to 119 µs and 24.4 KB. The new contract-based
decision no longer serialized an instance, so the MVC options stayed mutable, and a mutable
`JsonSerializerOptions.GetTypeInfo` builds a new `JsonTypeInfo` on every call. That defeated the
per-`JsonTypeInfo` cache.

The cache is now keyed by options and type, and the contract is only resolved when the decision is unknown or the
body must be shaped. Final allocation, compared with 014 `envelope-after.md`:

| Case | 014 after | 015 |
|---|---|---|
| MvcSuccess | 2.67 KB | 2.67 KB |
| AspNetCoreSuccess | 3.22 KB | 3.22 KB |
| MinimalSuccess | 3.00 KB | 3.00 KB |
| MvcPaged | 3.31 KB | 3.28 KB |
| AspNetCorePaged | 3.86 KB | 3.83 KB |
| MinimalPaged | 3.64 KB | 3.61 KB |

Allocation is the same or lower everywhere (SC-002). Mean times are within the ShortRun error margins; for example
`AspNetCorePaged` is 5.8 µs ± 30 µs.

## AOT check

The core was built with `-p:IsAotCompatible=true` (net10.0). It shows the same 6 pre-existing warnings (`ProblemResultJsonConverter` and `ResultReflections.IsUnwrappableType`/`GetDataType`) and no new one.

## Dependencies

- US1 (T001→T002), US2 (T003→T004) and US3 (T005→T006) are independent → Polish.
- T002 and T004 touch different files. T004 and T006 may both touch OpenApi files, so run T006 after T004.

## Parallel examples

- T001 ∥ T003 ∥ T005.
- T007 ∥ T008.

## Implementation strategy

The MVP is US1, the only runtime-output bug. Then US2, then US3. Commit docs, examples and code separately, as
usual. Text cleanups go to the docs commit (specs, READMEs) or to the code commit (src, tests, csproj release
notes), depending on the file.
