# Implementation Plan: Complete examples

**Branch**: `chore/add-spec-kit` | **Date**: 2026-11-23 | **Spec**: [spec.md](spec.md)

## Summary

Replace the weather forecast samples with an in-memory product catalog. The catalog demonstrates every
core result factory and the problem detail builder. The four example projects target .NET 10:

- The shared library holds the catalog used by both documentation hosts.
- **Scalar** (native OpenAPI) and **Swagger** (Swashbuckle) document the same endpoints, built both as
  controllers and as minimal APIs.
- A new **Mediators example** runs the catalog use cases as requests and handlers, with every mediator
  option and OpenTelemetry tracing and metrics.

Request files and a README cover everything. A smoke test project checks that every documented status can
be reached.

## Technical Context

**Language/Version**: C# latest, .NET 10 only (examples and the smoke tests)

**Primary Dependencies**:

- Repository packages through project references: Results, AspNetCore, Mvc, OpenApi,
  OpenApi.Swashbuckle, Mediators.
- Third-party packages already in use: Scalar.AspNetCore, Swashbuckle.AspNetCore,
  Microsoft.AspNetCore.OpenApi.
- New, examples only: OpenTelemetry.Extensions.Hosting and OpenTelemetry.Exporter.Console. Their versions
  are added to Directory.Packages.props.

**Storage**: in memory (a `ConcurrentDictionary`), reset on restart

**Testing**: xUnit with `Microsoft.AspNetCore.Mvc.Testing`, in the new project
`tests/Viana.Results.Examples.Tests` (smoke tests)

**Target Platform**: ASP.NET Core 10 web hosts

**Project Type**: example web apps plus a shared class library

**Constraints**:

- The solution builds with 0 warnings. TreatWarningsAsErrors is inherited; the examples are allowed only
  the existing CS1591 exception.
- `IsPackable=false` on every example project.
- No other mediator library may be named anywhere.

**Scale/Scope**: 4 example projects, about 25 source files, 1 test project

## Constitution Check

| Principle | Status |
|-----------|--------|
| I. Minimal, Portable Core | ✅ No library code changes; OpenTelemetry is used only in an example |
| II. Broad Compatibility | ✅ Library TFMs untouched; the examples only target net10 (by user decision) |
| III. Test-First | ✅ The smoke tests (status reachability, documentation coverage, mediator scenarios) are written before the endpoints |
| IV. RFC 9457 | ✅ Every failure is a ProblemDetails response; the examples show the native type link |
| V. Performance and Simplicity | ✅ No change to the hot path |
| VI. AOT (Core) | N/A, no core changes |
| VII. SemVer | ✅ No public API change; no package version bump |
| VIII. Documented Public Surface | ✅ The examples README is the tour; package READMEs keep pointing to the examples |
| IX. Native Interop | ✅ The examples use AddProblemDetails, the native OpenAPI, and native ActivitySource/Meter through OpenTelemetry |

Gate: PASS (checked again after design: PASS).

## Project Structure

```text
examples/
├── README.md                                  # new: feature → file → request map
├── Viana.Results.Examples.Shared/             # kept, rewritten around the catalog
│   ├── Catalog/Product.cs, ProductInput.cs, ProductCatalog.cs, InventoryGateway.cs
│   ├── Catalog/ProductRules.cs                # validation and business rules returning Result
│   ├── Controllers/ProductsController.cs      # native filter (AspNetCore)
│   ├── Controllers/LegacyProductsController.cs# Mvc interop (ToActionResult / Mvc filter)
│   ├── Controllers/ResultSamplesController.cs # kept: every factory and builder, one route each
│   ├── Endpoints/ProductEndpoints.cs          # minimal APIs with ProducesResult<T>
│   ├── Examples/*.cs                          # OpenAPI examples adapted to the catalog
│   └── CatalogServiceCollectionExtensions.cs  # AddCatalog()
├── Viana.Results.Scalar/                      # kept: Program.cs without the mediator, catalog.http
├── Viana.Results.Swagger/                     # kept: Program.cs without the mediator, catalog.http
└── Viana.Results.Mediators.Example/           # new web host
    ├── Program.cs                             # AddMediator(options) + OpenTelemetry + endpoints
    ├── UseCases/{Create,Get,List,Update,Delete}Product*.cs
    ├── UseCases/Scenarios/*.cs                # throwing handler, unavailable (503), slow cancellable, unregistered request
    ├── appsettings*.json                      # Mediator:Telemetry:* switches readable from configuration
    └── mediators.http
tests/Viana.Results.Examples.Tests/            # new: smoke tests over the three hosts
```

Removed: `WeatherForecast.cs`, `MinimalForecastEndpoints.cs`, `WeatherForecastController.cs`,
`Requests/CreateWeatherForecastRequest.cs`, `UseCases/WeatherForecast/*`, the weather examples, and
`Viana.Results.Swagger.http` (replaced by `catalog.http`). The Shared library drops its project reference
to Mediators.

**Structure Decision**: one example per integration, following the repository pattern (see the spec's
Project layout). The new projects are added to `Viana.Results.slnx` under `/examples/` and `/tests/`.

## Complexity Tracking

| Item | Why | Simpler alternative rejected |
|------|-----|------------------------------|
| Host marker classes (`ScalarHost`, `SwaggerHost`, `MediatorsHost`) | Three top-level `Program` types in one test project are ambiguous | `WebApplicationFactory<Program>` does not compile across three hosts |
| Smoke test project | Principle III, plus SC-002 and SC-003 need automated proof | Manual `.http` checks only: they cannot be enforced in CI |
