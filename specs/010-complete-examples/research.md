# Research: Complete examples

## R1 – Project layout
- **Decision**: Keep Shared, Scalar and Swagger; add a Mediators example host. All target .NET 10.
- **Rationale**: The user wants one example per integration, matching the repo pattern.
- **Alternatives**: One host with both UIs, which the user rejected.

## R2 – Showing the MVC interop next to the native filter
- **Decision**: A `LegacyProductsController` uses Viana.Results.Mvc (`ToActionResult`). `ProductsController`
  uses the AspNetCore filter.
- **Rationale**: Feature 006 made both packages coexist, and showing them side by side answers "which do I
  use?".
- **Alternatives**: A separate Mvc host. That adds a project for a single concept.

## R3 – OpenTelemetry wiring (Mediators example)
- **Decision**: Use `AddOpenTelemetry().WithTracing(t => t.AddSource(MediatorDiagnostics.SourceName).AddConsoleExporter())`
  and `.WithMetrics(m => m.AddMeter(MediatorDiagnostics.MeterName).AddConsoleExporter())`. Pin the latest
  stable OpenTelemetry versions centrally at implementation time.
- **Rationale**: The constants are the documented names, and the console exporter needs no infrastructure.
  The README notes that OTLP is a one-line swap.
- **Alternatives**: OTLP plus a collector or Aspire dashboard. That is heavier, so it is only mentioned.

## R4 – Configurable telemetry options
- **Decision**: Bind `MediatorOptions` from configuration in `Program.cs` using an `AddMediator(o => ...)`
  lambda that reads `builder.Configuration["Mediator:..."]`. Each option gets a comment, and the defaults
  stay visible.
- **Rationale**: Developers can toggle Tracing, Metrics and ExceptionDetails in appsettings and watch the
  effect without recompiling.

## R5 – Error scenarios
- **Decision**:
  - 500: a handler throws on purpose.
  - Missing handler: a request type with no handler, which raises `HandlerNotFoundException`. The endpoint
    catches it and returns `Results.Failure` (500), so the exception properties are visible.
  - 503: `InventoryGateway` has an availability toggle and returns a 503 problem.
  - Cancellation: a slow handler honors the token, and the endpoint passes `HttpContext.RequestAborted`.
- **Rationale**: Each scenario maps to a documented behavior from feature 009.

## R6 – Validation problem status
- **Decision**: Use the status the core `Results.Validation` already assigns. The examples do not override it.

## R7 – Smoke tests
- **Decision**: One `WebApplicationFactory<THost>` per host, each using its own marker class (`ScalarHost`,
  `SwaggerHost`, `MediatorsHost`), because three top-level `Program` types in one test project are ambiguous.
  The hosts use `UseExceptionHandler()` and `UseStatusCodePages()`, so exceptions become ProblemDetails. Use theory data with every route and expected status. Check that the OpenAPI document lists each
  status per operation. For the Mediators host, an in-memory `ActivityListener` asserts the span.
- **Rationale**: This automates SC-002 and SC-003 at low cost.
