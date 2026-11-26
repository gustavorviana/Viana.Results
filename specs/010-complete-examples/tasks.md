# Tasks: Complete examples

**Input**: Design documents in `specs/010-complete-examples/`
**Tests**: Required by constitution III. Smoke tests are written before the endpoints they cover.

## Phase 1: Setup

- [X] T001 Add the `OpenTelemetry.Extensions.Hosting` and `OpenTelemetry.Exporter.Console` versions (latest stable) to `Directory.Packages.props`.
- [X] T002 Create the web project `examples/Viana.Results.Mediators.Example/Viana.Results.Mediators.Example.csproj`. It targets net10.0 with `IsPackable=false` and references Results, AspNetCore, Mediators, OpenApi, Microsoft.AspNetCore.OpenApi, Scalar.AspNetCore and the two OpenTelemetry packages. Add `Properties/launchSettings.json` and `appsettings*.json`.
- [X] T003 Create the test project `tests/Viana.Results.Examples.Tests/Viana.Results.Examples.Tests.csproj`. It targets net10.0 and references xunit, Microsoft.AspNetCore.Mvc.Testing and the three host projects. Each host has its own global top-level `Program`, so the tests must not use `WebApplicationFactory<Program>`, which would be ambiguous. They use the per-host marker classes from T017/T024 instead (`ScalarHost`, `SwaggerHost`, `MediatorsHost`).
- [X] T004 Register both new projects in `Viana.Results.slnx` (folders `/examples/` and `/tests/`). Set `IsPackable=false` on `examples/Viana.Results.Scalar` and `examples/Viana.Results.Swagger` if it is missing.

## Phase 2: Foundational (catalog domain in Shared)

- [X] T005 Delete the weather samples: `examples/Viana.Results.Examples.Shared/WeatherForecast.cs`, `MinimalForecastEndpoints.cs`, `Controllers/WeatherForecastController.cs`, `Requests/CreateWeatherForecastRequest.cs`, `UseCases/WeatherForecast/*`, the weather examples in `Examples/`, and `examples/Viana.Results.Swagger/Viana.Results.Swagger.http`. Remove the Mediators reference from `Viana.Results.Examples.Shared.csproj` and add a reference to `Viana.Results.Mvc`.
- [X] T006 [P] Create `Catalog/Product.cs` and `Catalog/ProductInput.cs` in Shared. Fields: Id int; Sku string "required, 3–20 chars, unique (case-insensitive)"; Name "required, ≤ 100 chars"; Price decimal "> 0"; Stock int "≥ 0".
- [X] T007 [P] Create `Catalog/ProductRules.cs` in Shared. It validates a `ProductInput` and returns one `Results.Validation` problem that lists every invalid field.
- [X] T008 Create `Catalog/ProductCatalog.cs` in Shared. It is an in-memory store seeded with 3 products. SKU uniqueness must hold under concurrency: Create and Update run under a single `lock`, and SKUs are compared with `StringComparer.OrdinalIgnoreCase`, so two parallel creates with the same SKU yield exactly one 201 and one 409. It provides List, Get, Create, Update and Delete, all returning `Result`/`Result<T>`:
  - a duplicate SKU returns Conflict;
  - a missing id returns NotFound;
  - deleting a product with Stock > 0 returns BusinessRuleViolated.
- [X] T009 [P] Create `Catalog/InventoryGateway.cs` in Shared. Its `Available` toggle makes stock queries return a 503 problem built with the problem detail builder (`WithDetail`, `WithDescription`, an extension `retryAfterSeconds`).
- [X] T010 Create `CatalogServiceCollectionExtensions.cs` in Shared with `AddCatalog()`, which registers ProductCatalog and InventoryGateway as singletons.

## Phase 3: US1 + US2 – Result model through controllers and minimal APIs (P1, MVP)

- [X] T011 [US1] Write smoke tests in `tests/Viana.Results.Examples.Tests/CatalogRoutesTests.cs`. They use theory data over the Scalar and Swagger hosts and cover every route and status in `contracts/endpoints.md`, for both `/api/products` and `/minimal/products`:
  - status codes;
  - `application/problem+json`;
  - the RFC 9110 `type`;
  - the Location header on 201;
  - the validation errors body;
  - two parallel creates with the same SKU yield exactly one 201 and one 409.
- [X] T012 [US1] Write `tests/Viana.Results.Examples.Tests/SamplesRoutesTests.cs`. Every `/api/samples/{factory}` route, plus `/crash`, returns its expected status.
- [X] T013 [US2] Create `Controllers/ProductsController.cs` in Shared: CRUD plus `{id}/stock`, returning `Result`, using the AspNetCore filter. Add XML docs for OpenAPI.
- [X] T014 [P] [US2] Create `Controllers/LegacyProductsController.cs` in Shared. GET `/api/legacy/products/{id}` uses Viana.Results.Mvc `ToActionResult()`; add a comment explaining when to choose it.
- [X] T015 [P] [US2] Create `Endpoints/ProductEndpoints.cs` in Shared with `MapProductEndpoints()`. It provides the same operations under `/minimal/products` with `ProducesResult<T>` metadata, plus POST `/api/inventory/availability`.
- [X] T016 [US1] Rewrite `Controllers/ResultSamplesController.cs` in Shared with one route per core factory:
  - Ok, Ok with a value, implicit conversion, Created, NoContent;
  - BadRequest, Validation, Unauthorized, Forbidden, NotFound, Conflict, BusinessRuleViolated, Failure;
  - a builder route using WithDescription, WithDetail and extensions;
  - `/crash`, which throws.
- [X] T017 [US2] Update `examples/Viana.Results.Scalar/Program.cs` and `examples/Viana.Results.Swagger/Program.cs`:
  - remove AddMediator;
  - call `AddCatalog()` and `MapProductEndpoints()`;
  - keep AddProblemDetails and the Viana filter. The Mvc `ToActionResult()` needs no extra registration;
  - add `app.UseExceptionHandler()` and `app.UseStatusCodePages()`, so unhandled exceptions (`/crash`) become a 500 `application/problem+json` in every environment;
  - add a marker class (`ScalarHost` in `Viana.Results.Scalar`, `SwaggerHost` in `Viana.Results.Swagger`) for the tests' `WebApplicationFactory<T>`.

## Phase 4: US3 – Documentation in Scalar and Swagger (P2)

- [X] T018 [US3] Write `tests/Viana.Results.Examples.Tests/OpenApiCoverageTests.cs`. For each host, fetch the document (`/openapi/v1.json` and `/swagger/v1/swagger.json`) and assert that every operation lists every runtime status from the contract with a schema and an example, and that problem examples carry the RFC 9110 `type`.
- [X] T019 [US3] Adapt `Examples/*.cs` in Shared to the catalog: a product success example, a 409 duplicate-SKU example and the 503 inventory example. Register them, plus the existing 500 example, in both `Program.cs` files through `AddVianaResultExamples`.
- [X] T020 [P] [US3] Create `examples/Viana.Results.Scalar/catalog.http` and `examples/Viana.Results.Swagger/catalog.http`. They cover every route in both styles, the samples and the inventory toggle, using a `@host` variable.

## Phase 5: US4 – Mediators example with observability (P2)

- [X] T021 [US4] Write `tests/Viana.Results.Examples.Tests/MediatorsExampleTests.cs`. Test that:
  - CRUD statuses work;
  - `/scenarios/throws` returns 500;
  - `/scenarios/unregistered` returns 500 with the request type in the detail, and its span is Error with `error.type` = the `HandlerNotFoundException` full name;
  - with `Tracing=false` and `Metrics=false` set through configuration, no activity and no measurement are captured;
  - `/scenarios/unavailable` returns 503;
  - a cancelled `/scenarios/slow` yields a span that is not Error.

  An `ActivityListener` on `MediatorDiagnostics.SourceName` asserts the span name and the `viana.mediator.*` tags. `ExceptionDetails=TypeOnly` is set through configuration and leaves no message on the exception event.
- [X] T022 [P] [US4] Create the requests and handlers in `examples/Viana.Results.Mediators.Example/UseCases/`: `CreateProduct`, `GetProduct`, `ListProducts`, `UpdateProduct` and `DeleteProduct`. They use the shared `ProductCatalog` and return `Result`/`Result<T>`.
- [X] T023 [P] [US4] Create the scenario requests and handlers in `examples/Viana.Results.Mediators.Example/UseCases/Scenarios/`: `ThrowingRequest`, `UnavailableRequest` (through InventoryGateway), `SlowRequest` (`Task.Delay` with the token) and `UnregisteredRequest`, which has no handler on purpose.
- [X] T024 [US4] Write `examples/Viana.Results.Mediators.Example/Program.cs`:
  - Call `AddMediator(o => ...)` with every option commented: Telemetry.Tracing, Telemetry.Metrics and Telemetry.ExceptionDetails, read from the `Mediator` configuration section, with the defaults stated.
  - Configure `AddOpenTelemetry()` with a service resource, `WithTracing` (`AddSource(MediatorDiagnostics.SourceName)`, ASP.NET Core instrumentation not required, console exporter) and `WithMetrics` (`AddMeter(MediatorDiagnostics.MeterName)`, console exporter). Add a comment showing the OTLP swap.
  - Map the endpoints under `/products` and `/scenarios`, passing `RequestAborted`. Catch `HandlerNotFoundException` and return `Results.Failure` with RequestType and ResultType.
  - Add `AddProblemDetails()`, `app.UseExceptionHandler()` and `app.UseStatusCodePages()`, so the throwing handler becomes a 500 problem.
  - Add the native OpenAPI and Scalar, and a `MediatorsHost` marker class for the tests.
- [X] T025 [US4] Add a `Mediator` section to `examples/Viana.Results.Mediators.Example/appsettings.json` (`Telemetry:Tracing`, `Telemetry:Metrics`, `Telemetry:ExceptionDetails`) and create `mediators.http` covering every route, including the slow request.

## Phase 6: US5 – Guided tour (P3)

- [X] T026 [US5] Write `examples/README.md`:
  - how to run each host;
  - a feature → file → request table covering every core factory, the builder, the native filter, the Mvc interop, minimal APIs, OpenAPI with Scalar and with Swagger, the mediator options and the telemetry output;
  - how to toggle the telemetry options;
  - no other mediator library named.
- [X] T027 [P] [US5] Point the package READMEs (`src/*/README.md`, in a "Examples" line) and the root `README.md` to `examples/README.md`.

## Phase 7: Polish

- [X] T028 Run `dotnet build -c Release` (0 warnings) and `dotnet test` (whole suite, including the examples tests). Confirm that `dotnet pack` produces no example package.
- [X] T029 Follow `specs/010-complete-examples/quickstart.md` steps 3–6 manually, including SC-004 (a host running with console telemetry in under 5 minutes by following the README). Set the spec status to Ready. open both UIs and check the console telemetry and the option toggles. Check that no other mediator library is named in `examples/` or `specs/010-complete-examples/`.

## Follow-up (2026-11-24)

- [X] T030 One type per file in examples and their tests: split `Catalog/Product.cs` (Product, ProductInput, StockInfo), `Examples/CatalogExamples.cs` (one provider per file), the Mediators `UseCases` and `Scenarios` (one request or handler per file), move the host markers (`ScalarHost`, `SwaggerHost`, `MediatorsHost`) out of `Program.cs`, and `TelemetryCollection` out of `MediatorsExampleTests.cs`. Update the links in `examples/README.md`.

## Dependencies

- Setup (T001–T004) → Foundational (T005–T010) → US1/US2 (T011–T017) → US3 (T018–T020).
- US4 (T021–T025) depends only on the Foundational phase and can run in parallel with US1–US3.
- US5 comes after all stories; Polish comes last.
- Within each story, the tests (T011, T012, T018, T021) are written first and fail until the endpoints exist.

## Parallel examples

- Foundational: T006, T007 and T009 together.
- US2: T014 and T015 together once T013 has started the shared patterns.
- US4: T022 and T023 together.

## Implementation strategy

The MVP is the Scalar and Swagger hosts working on the catalog (Phases 1–3). Then add the documentation
(US3), the Mediators example (US4) and the README (US5).
