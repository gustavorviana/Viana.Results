# Tasks: Bodyless Result is 204, and WithoutDefaultResult

**Input**: Design documents in `specs/011-bodyless-result-204/`

**Tests**: Required by constitution III. Every test task comes before its code task and fails until that code exists.

## Phase 1: Setup

- [X] T001 Run `dotnet test` and record the baseline pass count. Then list the assertions that expect status 200 from `Results.Ok()`. Search for `Results.Ok()` combined with `200` / `StatusCodes.Status200OK` / `HttpStatusCode.OK` in `tests/**/*.cs` and note the list in this file under "Ok() assertions" (research R6).

## Phase 2: Foundational

- [X] T002 [P] Create `src/Viana.Results.OpenApi/WithoutDefaultResultAttribute.cs` as `public sealed class WithoutDefaultResultAttribute : Attribute`, with `[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]`. Its XML docs must say "last resort": only the hand-registered responses are documented, and runtime behavior is unchanged (contracts/public-api.md).
- [X] T003 [P] Add `WithoutDefaultResult()` to `src/Viana.Results.OpenApi/Endpoints/VianaRouteHandlerBuilderExtensions.cs` for `RouteHandlerBuilder`, `RouteGroupBuilder` and `IEndpointConventionBuilder`. It adds `new WithoutDefaultResultAttribute()` through `WithMetadata`/`Add`, follows the same overload pattern as `ProducesResult`, and has XML docs.

## Phase 3: US1 – A bodyless result is a 204, returned and documented (P1, MVP)

**Independent test**: an endpoint returning `Results.Ok()` answers 204. Its documents (native and Swashbuckle, controller and minimal) list 204 and no 200.

- [X] T004 [US1] Write the runtime tests:
  - `tests/Viana.Results.Tests/ResultsHelperTests.cs`: `Results.Ok().Status == 204`, `Problem == null`; `Results.Ok("x")` and `Results.Ok(value)` stay 200; `new Result(200)` stays 200.
  - `tests/Viana.Results.AspNetCore.Tests`: a controller action and a minimal API returning `Results.Ok()` answer 204 with no body.
  - `tests/Viana.Results.Mvc.Tests`: `VianaResultAction(Results.Ok())` answers 204.
- [X] T005 [US1] Write `tests/Viana.Results.OpenApi.Tests/DefaultResultTests.cs`. Each case runs on both generators (native document and Swashbuckle) through a TestServer:
  - (a) a controller action returning bare `Result` with no declaration lists `204` without content and no `200`;
  - (b) a minimal API returning `Result` lists `204` and no `200`;
  - (c) a minimal API with `.Produces(204)` lists `204` once and no `200`;
  - (d) a bare `Result` with `[ProducesResponseType(202)]` or `.Produces(202)` lists `202` only;
  - (e) `Result<T>` with no declaration keeps `200` with the schema of `T`;
  - (f) problem responses (`[ProblemResult]`, examples, the global 500) are unchanged in every case;
  - (g) `ProducesResult<P>()` where P returns `Results.Ok()` lists `204`;
  - (g2) a controller action returning `Result<T>` with `[ResponseExample(typeof(P))]`, where P returns `Results.Created(...)`, lists only `201` (no `200`), without `[ProducesResponseType]`;
  - (g3) a minimal API returning `Result<T>` with `ProducesResult<P>()` where P is a 202 result lists only `202`.
- [X] T006 [US1] Change `Results.Ok()` in `src/Viana.Results/Results.cs` to `return new Result(204);` and update its XML doc ("success without a body, HTTP 204 No Content"). This makes T004 green.
- [X] T007 [US1] In `src/Viana.Results.OpenApi/Processing/OperationProcessor.cs`, replace `ClearBodyForBareResult` with `internal static void ApplyDefaultResult(OpenApiOperation operation, MethodInfo method, IReadOnlyList<object> endpointMetadata)`, following research R2/R3:
  - compute the hand-declared 2xx statuses: `IProducesResponseTypeMetadata` whose `Type` is null, void or not a Viana result type; `[ProducesResponseType]` on the method or its declaring type; and 2xx statuses of the examples. For the examples, extract the status resolution that `ResponseExampleOperationTransformer` and the Swashbuckle example filter use today (`ResponseExampleResolver.Collect` for attributes, and the `VianaResponseMetadata` resolution for minimal APIs) into an internal helper shared with the example step (research R2b). It needs the same `IServiceProvider` and `VianaResultOptions`;
  - remove the inferred 2xx responses (status 200 when it is not hand-declared);
  - for a bare `Result` (after unwrapping `Task`/`ValueTask`) with no hand-declared 2xx, add `"204"` with description `"No Content"` and no content;
  - keep clearing the content of bodyless successes for a bare `Result`.

  Rename the public wrapper in `src/Viana.Results.OpenApi/Processing/OpenApiOperationProcessor.cs` to `ApplyDefaultResult(operation, method, endpointMetadata)`. Add XML remarks noting that `Produces<Result>(200)` cannot be told apart from the inferred 200, so `Produces(200)` or `ProducesResult` must be used instead.
- [X] T008 [US1] Pass the endpoint metadata, the `IServiceProvider` and the `VianaResultOptions` to the new step in both generators:
  - `src/Viana.Results.OpenApi/Transformers/NoResponseBodyOperationTransformer.cs` uses `context.Description.ActionDescriptor.EndpointMetadata`;
  - `src/Viana.Results.OpenApi.Swashbuckle/Filters/NoResponseBodyOperationFilter.cs` uses `context.ApiDescription.ActionDescriptor.EndpointMetadata`.

  Confirm that the registration order in `src/Viana.Results.OpenApi/OpenApiExtensions.cs` and `src/Viana.Results.OpenApi.Swashbuckle/SwaggerGenExtensions.cs` runs this step before the example and problem steps, and fix the order if it does not. This makes T005 green.
- [X] T009 [US1] Update the "Ok() assertions" listed in T001 from 200 to 204 across `tests/Viana.Results.Mediators.Tests`, `tests/Viana.Results.Mvc.Tests`, `tests/Viana.Results.OpenApi.Tests` (including `Swashbuckle/SwaggerPathsTests.cs`) and `tests/Viana.Results.Tests`, including the mediator telemetry status tag. The tests that use `Ok()` only as a generic success stay as they are.

## Phase 4: US2 – WithoutDefaultResult (P2)

**Independent test**: endpoints marked `WithoutDefaultResult` that register 200 and 202 by hand document exactly those two successes, in both styles and on both generators.

- [X] T010 [US2] Add the following cases to `tests/Viana.Results.OpenApi.Tests/DefaultResultTests.cs`, on both generators:
  - (h) a minimal API with `.WithoutDefaultResult()`, `ProducesResult<A>()` (200) and `ProducesResult<B>(202)` lists exactly 200 and 202 with their examples;
  - (i) the same for a controller action with `[WithoutDefaultResult]`, `[ResponseExample(typeof(A))]` and `[ResponseExample(typeof(B))]`;
  - (j) `[WithoutDefaultResult]` on the controller class applies to all of its actions;
  - (k) `WithoutDefaultResult` with no declared success lists no 2xx, and the problem responses are kept;
  - (l) `WithoutDefaultResult` on a route group applies to every endpoint in the group;
  - (m) runtime is unchanged: the marked endpoint still answers its result status.
- [X] T011 [US2] Make `ApplyDefaultResult` in `src/Viana.Results.OpenApi/Processing/OperationProcessor.cs` detect `endpointMetadata.OfType<WithoutDefaultResultAttribute>()`. When the marker is found, skip the default 204 and remove every 2xx that was not declared by hand. This makes T010 green.

## Phase 5: US3 – Examples (P3)

- [X] T012 [US3] Drop the redundant success declarations:
  - in `examples/Viana.Results.Examples.Shared/Endpoints/ProductEndpoints.cs`, remove `.Produces(StatusCodes.Status204NoContent)` from update, delete and the inventory toggle;
  - in `Controllers/ProductsController.cs` and `Controllers/ResultSamplesController.cs`, remove `[ProducesResponseType(StatusCodes.Status204NoContent)]`.

  Also remove the 201 workaround, `[ProducesResponseType<Product>(StatusCodes.Status201Created)]`, from `ProductsController.Create` and `ResultSamplesController.CreatedSample`: the `[ResponseExample]` that returns `Created` now documents 201 by itself (FR-005). The smoke test in T014 confirms the 201.
- [X] T013 [US3] Add a `WithoutDefaultResult` sample in each style. Each sample returns the base `Result` and, depending on a query flag, answers either 200 with a `Product` or 202 with a "reservation accepted" body. Each sample lives in one file per type (global rule):
  - controller: a new action `GET /api/samples/multi-success` in `Controllers/ResultSamplesController.cs` with `[WithoutDefaultResult]` and two `[ResponseExample]`;
  - minimal API: a new endpoint `GET /minimal/products/{id}/reservation` in `Endpoints/ProductEndpoints.cs` with `.WithoutDefaultResult()` and two `ProducesResult<...>`;
  - new providers in `Examples/ReservationAcceptedExample.cs` (202) and in a new `Catalog/Reservation.cs` record.

  Each sample carries a comment that says "last resort".
- [X] T014 [US3] Update the smoke tests:
  - in `tests/Viana.Results.Examples.Tests/SamplesRoutesTests.cs`, `ok` becomes 204; add `multi-success` (200 and 202);
  - in `tests/Viana.Results.Examples.Tests/OpenApiCoverageTests.cs`, add the new operations and assert that update, delete and availability have no `200`, and that the multi-success endpoints list exactly 200 and 202;
  - add the 202 route in `CatalogRoutesTests.cs`.
- [X] T015 [P] [US3] Update `examples/README.md`, `examples/Viana.Results.Scalar/catalog.http` and `examples/Viana.Results.Swagger/catalog.http`: describe the 204 default and add the `WithoutDefaultResult` rows and requests.

## Phase 6: Polish

- [X] T016 [P] Update the documentation:
  - in `src/Viana.Results/README.md`, `Results.Ok()` is 204;
  - in `src/Viana.Results.OpenApi/README.md` and `src/Viana.Results.OpenApi.Swashbuckle/README.md`, add the 204 default and a "WithoutDefaultResult (last resort)" section with the multi-success example;
  - in `src/Viana.Results.AspNetCore/README.md` and `src/Viana.Results.Mvc/README.md`, mention 204 for `Ok()`.
- [X] T017 [P] Update the release notes, without changing any version (spec FR-012), in the csproj of Viana.Results, Viana.Results.OpenApi, Viana.Results.OpenApi.Swashbuckle, Viana.Results.AspNetCore, Viana.Results.Mvc and Viana.Results.Mediators (telemetry status tag 204 for `Ok()`).
- [X] T018 Run `dotnet build -c Release` (0 warnings) and `dotnet test` (full suite). Then run quickstart.md steps 3–5. Check that no other mediator library is named and that every new type has its own file.

## Dependencies

- T001 → T002 ∥ T003 → US1 (T004, T005 → T006 → T007 → T008 → T009) → US2 (T010 → T011) → US3 (T012 → T013 → T014; T015 in parallel) → Polish (T016 ∥ T017 → T018).
- US2 depends on US1 because it reuses `ApplyDefaultResult`. US3 depends on both.

## Parallel examples

- T002 ∥ T003; T016 ∥ T017; T015 next to T014.

## Implementation strategy

The MVP is US1 (Phases 1–3): the runtime returns 204 and the documents stop showing the stray 200. Then
US2 adds the opt-out, and US3 adds the examples.

## Ok() assertions

Baseline (T001): full suite green (core 173, Mvc 153, AspNetCore 105×2, Mediators 60×2, OpenApi 105, Examples 83).
Assertions of 200 for `Results.Ok()` found by the run after T006:
- `tests/Viana.Results.Tests/ResultsHelperTests.cs` `Results_Ok_WithoutParameters_ReturnsSuccessResult` → 204
- `tests/Viana.Results.Mvc.Tests/VianaResultActionTests.cs` `ExecuteResultAsync_NonGenericResult_WithSuccess_DoesNotWriteBody` → 204
- `tests/Viana.Results.Mediators.Tests/MetricsTests.cs` `OneMeasurementPerSend_WithTags` (status tag) → 204
- `tests/Viana.Results.OpenApi.Tests/OperationProcessorTests.cs` `ClearBodyForBareResult_HandlesTaskAndValueTask` → `ApplyDefaultResult_HandlesTaskAndValueTask` (204)
- examples `SamplesRoutesTests` `ok` → 204 (T014)
