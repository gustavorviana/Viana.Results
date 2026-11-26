# Research: Review leftovers

## R1 – Why the ProblemResult provider is lost on minimal APIs
- `Resolve(VianaResponseMetadata)` handles `IResult` values. Any other value falls through to
  `new ResolvedDeclaration(decl, decl.Status ?? 200, hasExample, Problem: null, value, value != null)`. A
  `ProblemResult` therefore becomes a **200 success body**, which overwrites or merges into the real 200 instead
  of producing its own problem status.
- **Decision**: add `value is ProblemResult problem` and return
  `new ResolvedDeclaration(decl, problem.Status, true, problem, problem, true)`. An explicit `Status` that differs
  throws, as the result branch already does. When `IsProblem` and the value is neither `IResult` nor
  `ProblemResult` (with a non-null value), throw `InvalidOperationException`:
  `"Endpoint '{name}': problem example provider '{type}' must return a Viana result or a ProblemResult."`
- **Alternative**: reject `ProblemResult` providers on minimal APIs. Rejected, because controllers accept them
  (FR-003 parity).

## R2 – Mvc README
- The Mvc package exposes `VianaResultAction(IResult)` (an `ActionResult`) and `AddVianaResultFilter()`, but no
  `ToActionResult()`.
- **Decision**: the snippets use `new VianaResultAction(result)`, plus a note: "`ToActionResult()` lives in
  Viana.Results.AspNetCore". `tests/Viana.Results.Mvc.Tests/Readme/ReadmeUsersController.cs` (and one file per
  type it needs) mirrors the snippets, so a README regression shows up as a compile error.

## R3 – Duration buckets
- OpenTelemetry semantic conventions recommend `[0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5,
  5, 7.5, 10]` for seconds-based HTTP durations. The SDK honors `InstrumentAdvice.HistogramBucketBoundaries` when
  no View overrides it.
- **Decision**: `public static IReadOnlyList<double> DurationBucketBoundaries` in `MediatorDiagnostics` (all
  TFMs, read-only). On net9+ the histogram is created through
  `Meter.CreateHistogram<double>(name, unit, description, tags: null, advice: new() { HistogramBucketBoundaries = ... })`.
  The README shows, for older targets:
  `metrics.AddView(MediatorDiagnostics.DurationInstrument, new ExplicitBucketHistogramConfiguration { Boundaries = MediatorDiagnostics.DurationBucketBoundaries.ToArray() })`.
- **Test**: `MeterListener.InstrumentPublished` receives the instrument, and on net10 the test asserts that
  `((Histogram<double>)instrument).Advice!.HistogramBucketBoundaries` equals the constant.
