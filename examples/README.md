# Viana.Results examples

The examples are runnable .NET 10 apps built around one small domain: an in-memory **product catalog**.
Each Viana package has its own example, and the core `Viana.Results` package is used by all of them.

| Project | Shows | Run | Requests |
|---|---|---|---|
| [Viana.Results.Examples.Shared](Viana.Results.Examples.Shared/) | The catalog, the controllers and the minimal APIs used by the two hosts below | (library) | — |
| [Viana.Results.Scalar](Viana.Results.Scalar/) | Native OpenAPI (`Viana.Results.OpenApi`) with the Scalar UI | `dotnet run --project examples/Viana.Results.Scalar` → http://localhost:5027 | [catalog.http](Viana.Results.Scalar/catalog.http) |
| [Viana.Results.Swagger](Viana.Results.Swagger/) | Swashbuckle (`Viana.Results.OpenApi.Swashbuckle`) with Swagger UI | `dotnet run --project examples/Viana.Results.Swagger` → http://localhost:5026 | [catalog.http](Viana.Results.Swagger/catalog.http) |
| [Viana.Results.Mediators.Example](Viana.Results.Mediators.Example/) | `Viana.Results.Mediators`: requests, handlers, options and OpenTelemetry | `dotnet run --project examples/Viana.Results.Mediators.Example` → http://localhost:5031 | [mediators.http](Viana.Results.Mediators.Example/mediators.http) |

Opening `/` redirects to the API UI. The catalog starts with 3 products and resets when the app restarts.
[tests/Viana.Results.Examples.Tests](../tests/Viana.Results.Examples.Tests/) checks every route below on
every host.

## Where each feature is

### Core results (`Viana.Results`)

| Feature | File | Request |
|---|---|---|
| `Results.Ok()`, `Ok(string)`, `Ok(value)` | [ResultSamplesController.cs](Viana.Results.Examples.Shared/Controllers/ResultSamplesController.cs) | `GET /api/samples/ok`, `/ok-message`, `/ok-value` |
| Implicit conversion value → `Result<T>` | [ProductCatalog.cs](Viana.Results.Examples.Shared/Catalog/ProductCatalog.cs) (`Get`), samples | `GET /api/samples/implicit` |
| `Results.Created`, `Results.NoContent` | catalog `Create`, `Update`, `Delete` | `POST /api/products`, `PUT`/`DELETE /api/products/{id}` |
| `ListResult<T>`, `PagedResult<T>.Create` | samples, `ProductsController.List` | `GET /api/samples/list`, `/paged?page=2` |
| `BadRequest`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict` | samples, catalog | `GET /api/samples/bad-request` … `/conflict`; `GET /api/products/99` |
| `Results.Validation(errors)` | [ProductRules.cs](Viana.Results.Examples.Shared/Catalog/ProductRules.cs) | `POST /api/products` with an invalid body |
| `Results.BusinessRuleViolated` (422) | catalog `Delete` | `DELETE /api/products/1` (it still has stock) |
| `Results.Failure(status)`, `Results.Failure(exception)` | samples | `GET /api/samples/failure`, `/failure-exception` |
| `ProblemBuilder` (title, type, detail, instance, extensions) | samples `Builder`, [InventoryGateway.cs](Viana.Results.Examples.Shared/Catalog/InventoryGateway.cs) | `GET /api/samples/builder`; `GET /api/products/1/stock` after turning the inventory off |
| `ProblemResult` converted implicitly to `Result<T>` | samples | `GET /api/samples/problem-result` |
| A failed `Result` passed through as `Result<T>` | catalog `Create` and `Update` (validation) | — |

### ASP.NET Core

| Feature | File | Request |
|---|---|---|
| Controllers returning `Result` (`AddVianaResultFilter`, `Viana.Results.AspNetCore`) | [ProductsController.cs](Viana.Results.Examples.Shared/Controllers/ProductsController.cs) | `/api/products` |
| Minimal APIs (`AddVianaResults`, `ProducesResult`, `ProducesProblemResult`) | [ProductEndpoints.cs](Viana.Results.Examples.Shared/Endpoints/ProductEndpoints.cs) | `/minimal/products` |
| `Viana.Results.Mvc` (`VianaResultAction`) in the same app | [LegacyProductsController.cs](Viana.Results.Examples.Shared/Controllers/LegacyProductsController.cs) | `GET /api/legacy/products/1` |
| Native problem details for exceptions and bad requests (`AddProblemDetails`, `UseExceptionHandler`, `UseStatusCodePages`) | `Program.cs` of each host | `GET /api/samples/crash`; malformed JSON |

The controller and minimal API routes give the same statuses and bodies. Failures are
`application/problem+json`, and their `type` is the RFC 9110 link that ASP.NET Core sends.

### OpenAPI

| Feature | File |
|---|---|
| `[ResponseExample]` with providers that return the real result | [Examples/](Viana.Results.Examples.Shared/Examples/) (one provider per file), `ProductsController` |
| `[ProblemResult(status, Title, Type, Detail)]` | `ResultSamplesController` |
| Global 500 example (`AddVianaResultExamples`) | [InternalServerErrorExample.cs](Viana.Results.Examples.Shared/Examples/InternalServerErrorExample.cs), host `Program.cs` |
| Native document + Scalar (`AddVianaResultTransformers`) | [Viana.Results.Scalar/Program.cs](Viana.Results.Scalar/Program.cs) → `/openapi/v1.json` |
| Swashbuckle + Swagger UI (`AddVianaResultFilters`, XML comments) | [Viana.Results.Swagger/Program.cs](Viana.Results.Swagger/Program.cs) → `/swagger/v1/swagger.json` |

### Mediators and observability

| Feature | File | Request |
|---|---|---|
| Requests and handlers returning results | [UseCases/](Viana.Results.Mediators.Example/UseCases/) (one request or handler per file) | `/products` |
| Every `AddMediator` option, read from configuration | [Program.cs](Viana.Results.Mediators.Example/Program.cs), [appsettings.json](Viana.Results.Mediators.Example/appsettings.json) | — |
| Traces and metrics with OpenTelemetry (console exporter) | `Program.cs` | any call |
| Handler that throws (500, span `Error`, exception event) | [UseCases/Scenarios/](Viana.Results.Mediators.Example/UseCases/Scenarios/) | `GET /scenarios/throws` |
| `HandlerNotFoundException` (`RequestType`, `ResultType`) | `Program.cs` | `GET /scenarios/unregistered` |
| 5xx result marks the span as `Error` | scenarios | `GET /scenarios/unavailable` |
| Cancellation is not an error (`viana.mediator.cancelled`) | scenarios | `GET /scenarios/slow?seconds=10`, cancelled from the client |

## Mediator telemetry

The mediator writes to the native `ActivitySource` and `Meter` named `Viana.Results.Mediators`
(`MediatorDiagnostics.SourceName` / `MeterName`), so subscribing by name is all OpenTelemetry needs:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddSource("Microsoft.AspNetCore")          // the request span (parent of the mediator span)
        .AddSource(MediatorDiagnostics.SourceName)
        .AddConsoleExporter())
    .WithMetrics(m => m.AddMeter(MediatorDiagnostics.MeterName).AddConsoleExporter());
```

The default sampler follows the parent span. In a web app the mediator span is a child of the ASP.NET
Core request span, so that span must be recorded too: add the `Microsoft.AspNetCore` source (or the
ASP.NET Core instrumentation package). Otherwise the mediator spans are dropped and only the metrics show.

Run the example, call any route, and the console prints:

- a span `Mediator.Send GetProduct` with the tags `viana.mediator.request.type`, `viana.mediator.result.type`,
  `viana.mediator.handler.type`, `viana.mediator.result.success`, `viana.mediator.result.status`, and also
  `error.type` on failures and `viana.mediator.cancelled` on cancellation;
- the histogram `viana.mediator.request.duration` (seconds) with the request type and status dimensions.

To send them to a real backend, replace `AddConsoleExporter()` with `AddOtlpExporter()` (package
`OpenTelemetry.Exporter.OpenTelemetryProtocol`).

### Changing the options

Every option is in the `Mediator` section of
[appsettings.json](Viana.Results.Mediators.Example/appsettings.json). They can also be overridden on the
command line, for example:

```bash
dotnet run --project examples/Viana.Results.Mediators.Example -- --Mediator:Telemetry:ExceptionDetails=TypeOnly
```

| Setting | Default | Effect |
|---|---|---|
| `Mediator:Telemetry:Tracing` | `true` | `false`: no spans, even with OpenTelemetry listening |
| `Mediator:Telemetry:Metrics` | `true` | `false`: no duration measurements |
| `Mediator:Telemetry:ExceptionDetails` | `Full` | `TypeOnly`: exception type only; `None`: no exception event (span stays `Error`, `error.type` kept) |
| `Mediator:ConsoleExporter` | `true` | Example-only switch for the console exporter |

Messages and stack traces can contain sensitive data, so use `TypeOnly` or `None` where that matters.
