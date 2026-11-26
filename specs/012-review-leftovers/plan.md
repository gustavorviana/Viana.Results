# Implementation Plan: Review leftovers

**Branch**: `chore/add-spec-kit` | **Date**: 2026-11-25 | **Spec**: [spec.md](spec.md)

## Summary

The feature has three small, independent changes.

1. **OpenApi (US1).** `OperationProcessor.Resolve` (minimal API declarations) gets a `ProblemResult` branch:
   status, `Problem` and body come from the problem, which is exactly how `BuildExamples` already treats
   controller attributes. A problem declaration whose value is neither `IResult` nor `ProblemResult` throws an
   `InvalidOperationException` naming the endpoint and the provider.
2. **Mvc README (US2).** Replace `result.ToActionResult()` with `new VianaResultAction(result)` and add a pointer
   to Viana.Results.AspNetCore. A test file in `tests/Viana.Results.Mvc.Tests` mirrors every snippet, so the build
   proves they compile.
3. **Mediators (US3).** `MediatorDiagnostics.DurationBucketBoundaries` is a public `IReadOnlyList<double>`. Under
   `#if NET9_0_OR_GREATER`, the histogram is created with
   `new InstrumentAdvice<double> { HistogramBucketBoundaries = ... }`. The README documents the OpenTelemetry View
   for net6/net8.

## Technical Context

**Language/Version**: C# latest. OpenApi and Swashbuckle target net10.0. Mvc targets net6/8/10. Mediators
targets netstandard2.0, net6, net8 and net10.
**Primary Dependencies**: none new. `InstrumentAdvice<T>` is inbox in System.Diagnostics.DiagnosticSource 9+,
which ships with net10.
**Testing**: xUnit, in `OpenApi.Tests` (both generators, document level), `Mvc.Tests` (snippets compile; one
type per file) and `Mediators.Tests` (advice on net10, through `Instrument.Advice`).
**Constraints**: 0 warnings. No version bump. Release notes go in the OpenApi, Swashbuckle, Mvc and Mediators
csproj files.

## Constitution Check

| Principle | Status |
|-----------|--------|
| I. Minimal core | ✅ The core is untouched |
| II. Compatibility | ✅ Advice only under `NET9_0_OR_GREATER`; older targets are unchanged |
| III. Test-First | ✅ Tests first in every story |
| IV. RFC 9457 | ✅ Problem providers documented as RFC 9457 problems |
| V. Performance | ✅ The advice is set once at instrument creation; the hot path is unchanged |
| VI. AOT (core) | N/A |
| VII. SemVer | ✅ Nothing is published; release notes only |
| VIII. Documented surface | ✅ XML doc for the new constant; READMEs fixed |
| IX. Native interop | ✅ Uses the native `InstrumentAdvice` |

Gate: PASS. Re-checked after design: PASS.

## Project Structure

```text
src/Viana.Results.OpenApi/Processing/OperationProcessor.cs      # Resolve: ProblemResult branch + error for unknown values
src/Viana.Results.Mvc/README.md                                 # real API
src/Viana.Results.Mediators/MediatorDiagnostics.cs              # DurationBucketBoundaries
src/Viana.Results.Mediators/Telemetry/MediatorTelemetry.cs      # advice under NET9_0_OR_GREATER
src/Viana.Results.Mediators/README.md                           # buckets + View for net6/net8
tests/Viana.Results.OpenApi.Tests/ProblemProviders/*.cs         # one type per file
tests/Viana.Results.Mvc.Tests/Readme/*.cs                       # README snippets as code
tests/Viana.Results.Mediators.Tests/DurationAdviceTests.cs
examples/Viana.Results.Examples.Shared/Examples/ServiceUnavailableExample.cs   # back to IExampleProvider<ProblemResult>
```

## Complexity Tracking

None.
