# Tasks: Review leftovers

**Input**: Design documents in `specs/012-review-leftovers/`
**Tests**: Required by constitution III. Tests are written before the code they cover. Test files hold one type per file.

## Phase 1: Setup

- [X] T001 Run `dotnet test` once to record a green baseline (no code changes).

## Phase 2: Foundational

No shared prerequisites. The three stories are independent.

## Phase 3: US1 – ProblemResult providers on minimal APIs (P1, MVP)

**Independent test**: `ProducesProblemResult<P>()`, where P returns a 503 `ProblemResult`, lists a 503 `application/problem+json` response with P's example in both generators.

- [X] T002 [P] [US1] Create the test providers in `tests/Viana.Results.OpenApi.Tests/ProblemProviders/`, one per file:
  - `UnavailableProblem.cs`: `IExampleProvider<ProblemResult>`, 503, title "Unavailable", detail, extension `retryAfterSeconds = 5`;
  - `NotAResult.cs`: `IExampleProvider<string>`, returns "x".
- [X] T003 [US1] Write `tests/Viana.Results.OpenApi.Tests/ProblemProviders/ProblemProviderTests.cs`. It uses `OperationProcessor.ApplyEndpointDeclarations` with a stub builder, as `ProblemTypeDefaultsTests` does, and also a real app on both generators, as `DefaultResult/DefaultResultHost` does. The tests check that:
  - (a) `ProducesProblemResult<UnavailableProblem>()` documents 503 with the `application/problem+json` content, and the example has title, detail and `retryAfterSeconds`;
  - (b) `ProducesResult<UnavailableProblem>()` documents the same 503 problem and adds no 200;
  - (c) the inline form `ProducesProblemResult(new Result(problem))` (an `IResult` wrapping a `ProblemResult`) documents the same 503. No new overload is added (FR-001);
  - (d) `ProducesProblemResult<NotAResult>()` throws `InvalidOperationException`, and the message contains the endpoint name and `NotAResult`;
  - (e) `ProducesResult<UnavailableProblem>(status: 500)` throws, because the explicit status differs from the provider's 503;
  - (f) the controller `[ResponseExample(typeof(UnavailableProblem))]` and the minimal API document the same status and example (FR-003).
- [X] T004 [US1] In `src/Viana.Results.OpenApi/Processing/OperationProcessor.cs` `Resolve`, add the `ProblemResult` branch after the `IResult` branch (research R1):
  - check the explicit status the same way the `IResult` branch does;
  - return `new ResolvedDeclaration(declaration, problem.Status, true, problem, problem, true)`;
  - then, when `declaration.IsProblem && hasExample && value is not null`, throw `InvalidOperationException($"Endpoint '{endpointName}': problem example provider '{type}' must return a Viana result or a ProblemResult.")`.

  This makes T003 green.
- [X] T005 [US1] Turn `examples/Viana.Results.Examples.Shared/Examples/ServiceUnavailableExample.cs` back into an `IExampleProvider<ProblemResult>` using `ProblemResult.WithDetail(503, "Inventory unavailable", detail, type, extensions)`. Confirm that `tests/Viana.Results.Examples.Tests` still passes, with 503 documented on both stock routes.

## Phase 4: US2 – Mvc README matches the package (P2)

**Independent test**: every Mvc README snippet compiles in `tests/Viana.Results.Mvc.Tests/Readme/`.

- [X] T006 [P] [US2] Mirror every README snippet in `tests/Viana.Results.Mvc.Tests/Readme/`, one type per file:
  - `ReadmeUser.cs`, `ReadmeCreateUserDto.cs`, `IReadmeUserService.cs`;
  - `ReadmeUsersController.cs` with `GetUser` and `CreateUser` returning `new VianaResultAction(result)`, and `GetUsers` returning `IResult` through the implicit conversion;
  - `ReadmeFilterRegistration.cs` with both registrations: `services.AddControllers().AddVianaResultFilter()`, and the manual `services.AddControllers(o => o.Filters.Add<VianaResultFilter>())` using `Viana.Results.Mvc.Filters`.

  The test project references only Viana.Results.Mvc, so a `ToActionResult()` call fails to compile. That is the guard.
- [X] T007 [US2] Fix `src/Viana.Results.Mvc/README.md`:
  - replace `result.ToActionResult()` with `new VianaResultAction(result)` in both actions;
  - add a note after the snippet: "`ToActionResult()` is in Viana.Results.AspNetCore";
  - keep the snippets identical to the T006 files.

## Phase 5: US3 – Duration buckets in seconds (P2)

**Independent test**: on net10 the duration instrument carries advice with the 14 boundaries. The bucket distribution (SC-003) is checked by hand in quickstart step 4.

- [X] T008 [P] [US3] Write `tests/Viana.Results.Mediators.Tests/DurationAdviceTests.cs`:
  - (a) `MediatorDiagnostics.DurationBucketBoundaries` equals `[0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]`, on all TFMs;
  - (b) under `#if NET9_0_OR_GREATER`, a `MeterListener` that captures the `viana.mediator.request.duration` instrument, after a dispatch, finds `((Histogram<double>)instrument).Advice!.HistogramBucketBoundaries` equal to the constant.

  It joins the `TelemetryCollection`, because listeners are process-wide.
- [X] T009 [US3] Add `public static IReadOnlyList<double> DurationBucketBoundaries { get; }` to `src/Viana.Results.Mediators/MediatorDiagnostics.cs`, backed by a private static array and documented with XML docs. In `src/Viana.Results.Mediators/Telemetry/MediatorTelemetry.cs`, create the histogram with `advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = MediatorDiagnostics.DurationBucketBoundaries }` under `#if NET9_0_OR_GREATER`, and keep the current call otherwise. This makes T008 green.
- [X] T010 [US3] In `src/Viana.Results.Mediators/README.md` (Telemetry section), state that on .NET 9+ the buckets are advised, and give net6/net8 the View `metrics.AddView(MediatorDiagnostics.DurationInstrument, new ExplicitBucketHistogramConfiguration { Boundaries = MediatorDiagnostics.DurationBucketBoundaries.ToArray() })`.

## Phase 6: Polish

- [X] T011 [P] Release notes, without version changes, in `src/Viana.Results.OpenApi/Viana.Results.OpenApi.csproj` and `src/Viana.Results.OpenApi.Swashbuckle/Viana.Results.OpenApi.Swashbuckle.csproj` (ProblemResult providers on minimal APIs, and a clear error), `src/Viana.Results.Mvc/Viana.Results.Mvc.csproj` (README fixed) and `src/Viana.Results.Mediators/Viana.Results.Mediators.csproj` (bucket advice, `DurationBucketBoundaries`). Remove the three resolved items from the memory file `examples-review-findings.md`.
- [X] T012 Run `dotnet build -c Release` (0 warnings) and `dotnet test`, then quickstart steps 3–4. Check that no other mediator library is named, and that the new test and example files hold one type per file.

## Dependencies

- T001 → US1 (T002 → T003 → T004 → T005), US2 (T006 → T007) and US3 (T008 → T009 → T010). The three stories are independent → Polish (T011 → T012).

## Parallel examples

- T002 ∥ T006 ∥ T008, and also the three stories as a whole.

## Implementation strategy

The MVP is US1 (the only behavior fix in the OpenAPI output). US2 and US3 are small and independent.
