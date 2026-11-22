# Research: ASP.NET Core Package with Native ProblemDetails Interop

## R1 – Writing failures with precedence Viana → native defaults → CustomizeProblemDetails

- **Decision**: Build a native `ProblemDetails` from the Viana problem (Status, Title, Type, Detail,
  Instance, Extensions), then call `IProblemDetailsService.TryWriteAsync(new ProblemDetailsContext {
  HttpContext, ProblemDetails })`. The framework writers apply defaults only to missing values (type
  link per status, title, `traceId` added only if absent) and then invoke
  `ProblemDetailsOptions.CustomizeProblemDetails`. MVC's writer uses MVC formatters, so Newtonsoft apps
  keep their serializer.
- **Fallback** (no `AddProblemDetails()`, or `TryWriteAsync` returns false):
  `ProblemDetailsFactory.CreateProblemDetails(httpContext, status, title, type, detail, instance)`,
  copy Viana extensions on top, then `ObjectResult` with content type `application/problem+json`.
- **Verification gate**: the first implementation task is a spike test asserting that (a) a Viana
  `traceId` extension survives, (b) a missing type gets the native link, (c) customization runs after
  the Viana values and can overwrite them, (d) customization runs exactly once. If the writer does not
  behave this way on net8.0, use the fallback path for all cases and document it.
- **Alternatives**: factory only (customization would run before Viana extensions are merged, which
  breaks "customization last"); writing JSON manually like the old package (no native defaults,
  ignores Newtonsoft).

## R2 – Viana `type` default versus the native type link

- **Decision**: Viana `ProblemResult.Type` defaults to `about:blank`. When it is `about:blank`, map it to
  `null` so the native default (RFC 9110 link per status) applies; any other value is explicit and wins.
- **Rationale**: Acceptance scenario US1.1 (native type link when none is given) and FR-003.

## R3 – Detail and instance

- **Decision**: Viana stores `detail`/`instance` (and `description`) as extensions; the mapper promotes
  `detail` and `instance` to the native properties and copies the remaining extensions as-is
  (including `description` and `errors`).

## R4 – Success formatting

- **Decision**: `ObjectResult(ResultResponseBody.GetBody(result)) { StatusCode = result.Status }`;
  when the body is null, `StatusCodeResult(result.Status)`.
- **Rationale**: Reuses the core body-shape rules (FR-007) and MVC negotiation (FR-006).

## R5 – Public surface

- **Decision**: `VianaActionResult` (public, sealed), `ToActionResult(this IResult)` and the
  `Task<TResult>` overload, and `IMvcBuilder.AddVianaResultFilter()`; namespace
  `Viana.Results.AspNetCore`. The old package's `Headers`/`ContentType` virtuals are not carried over
  (spec assumption).

## R6 – Test dependencies

- **Decision**: Add central versions for `Microsoft.AspNetCore.Mvc.NewtonsoftJson` (per TFM: 8.0.x for
  net8.0, 10.0.x for net10.0), referenced only by the test project.

## R7 – Benchmark

- **Decision**: `AspNetCoreBenchmarks` executes `VianaActionResult` (success and failure) against a
  `DefaultHttpContext` with a minimal service provider, side by side with the old `VianaResultAction`.
