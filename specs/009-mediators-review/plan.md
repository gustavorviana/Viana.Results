# Implementation Plan: Mediators Review

**Branch**: `chore/add-spec-kit` (shared) | **Date**: 2026-11-23 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/009-mediators-review/spec.md`

## Summary

- **Dispatch**: replace the per-instance `ConcurrentDictionary<Type, HandlerInfo>` + `MethodInfo.Invoke` with
  a typed wrapper `HandlerWrapper<TRequest, TResult>` cached process-wide in `DispatchCache<TResult>`
  (static generic class, keyed by runtime request type → result type is part of the key by construction).
  Built once with a `static` factory (no closure); afterwards a dictionary lookup + virtual call.
  No-telemetry path returns the handler task directly (no extra state machine); errors still surface
  through the returned task (`Task.FromException`, FR-018).
- **Telemetry** (net6.0+): internal `MediatorTelemetry` with a static `ActivitySource` and `Meter`
  (public names in `MediatorDiagnostics`), `viana.mediator.*` tags, status rules (5xx/exception → Error,
  cancellation → not error), exception recording by detail level (`Activity.AddException` on net9+),
  one histogram `viana.mediator.request.duration`. Data built only when `activity.IsAllDataRequested` /
  `histogram.Enabled`; switches in options skip the source/meter entirely.
- **Registration**: `AddMediator(Action<MediatorOptions>, params Assembly[])` overload; `TryAdd` for the
  mediator and options; loadable-type scanning; skip abstract/open generic; duplicate handler → error;
  configurable handler lifetime.
- **Errors**: `HandlerNotFoundException` names the request type, new `RequestType`/`ResultType`
  properties; null task → `InvalidOperationException` naming handler and request.
- **Benchmark**: `MediatorBenchmarks` (direct handler call, dispatch with/without listener, telemetry off),
  baseline recorded **before** changing the code.

## Technical Context

**Language/Version**: C# latest, .NET SDK 10

**Primary Dependencies**: none new (`Microsoft.Extensions.DependencyInjection.Abstractions` as today;
`System.Diagnostics.Metrics`/`ActivitySource` are in the BCL for net6.0+)

**Storage**: N/A

**Testing**: xUnit (tests/Viana.Results.Mediators.Tests, `net8.0;net10.0`), `ActivityListener` and `MeterListener` for
telemetry assertions; BenchmarkDotNet

**Target Platform**: netstandard2.0, net6.0, net8.0, net10.0 (telemetry only net6.0+, as today)

**Project Type**: library (NuGet)

**Performance Goals**: SC-001 (≥ 50% fewer allocations, faster), SC-002/SC-006 (no telemetry cost without
listener or when disabled)

**Constraints**: TreatWarningsAsErrors; public contracts unchanged (FR-014); no new dependency; docs never
mention other mediator libraries

**Scale/Scope**: ~6 source files changed/added, ~35 tests, 1 benchmark, README

## Constitution Check

| Principle | Status | Note |
|-----------|--------|------|
| I. Minimal core | PASS | Core untouched |
| II. TFMs | PASS | Same targets; telemetry stays net6.0+ (netstandard2.0 telemetry deferred by the user) |
| III. TDD | PASS | Bug/telemetry/registration tests first; benchmark baseline before code |
| IV. RFC 9457 | PASS | Status from `Result.Status`/problem, no change to problem shape |
| V. Performance | PASS | Hot path rewritten with before/after benchmark (mediator dispatch is a named hot path) |
| VI. AOT | N/A | Mediators is not the core; AOT for Mediators is the next spec (user decision D3) |
| VII. SemVer | PASS (justified) | Tag renames and duplicate-handler error approved by the user (O8, U5) and treated as corrections: the current tags never matched the README and duplicate handlers were an undefined, silent behavior. Additive APIs otherwise → **1.1.0** |
| VIII. Docs | PASS | README rewritten (TFMs, telemetry, options, names) |
| IX. Native interop | PASS | Native `ActivitySource`/`Meter`/`Activity.AddException`; OpenTelemetry naming |

Post-design re-check: PASS.

## Project Structure

```text
specs/009-mediators-review/ (plan, research, data-model, quickstart, contracts/public-api.md, tasks)

src/Viana.Results.Mediators/
├── Mediator.cs                       # SendAsync: cache lookup, fast path, telemetry path
├── Dispatch/HandlerWrapper.cs        # internal abstract HandlerWrapper<TResult> + HandlerWrapper<TRequest,TResult>
├── Dispatch/DispatchCache.cs         # internal static DispatchCache<TResult>
├── MediatorOptions.cs                # public: Telemetry (HandlerLifetime removed, Phase 9)
├── MediatorTelemetryOptions.cs       # public: Tracing, Metrics, ExceptionDetails
├── ExceptionDetailLevel.cs           # public enum Full | TypeOnly | None
├── MediatorDiagnostics.cs            # public constants: SourceName, MeterName, tag names
├── Telemetry/MediatorTelemetry.cs    # internal (net6.0+): source, meter, start/finish, exception recording
├── HandlerNotFoundException.cs       # message + RequestType/ResultType
├── ServiceCollectionExtensions.cs    # options overload, TryAdd, scanning, duplicates, lifetime
├── (removed) MediatorActivity.cs, ActivityExtension.cs
└── README.md, csproj (1.1.0 + release notes)

tests/Viana.Results.Mediators.Tests/
├── DispatchTests.cs, TelemetryTests.cs, MetricsTests.cs, RegistrationTests.cs, OptionsTests.cs

benchmarks/Viana.Results.Benchmarks/
├── MediatorBenchmarks.cs
└── baseline/mediators-1.0.0.md       # before; updated with after
```

**Structure Decision**: all changes inside the Mediators package, its tests and the benchmark project
(gains a reference to Mediators).

## Complexity Tracking

| Item | Why needed | Simpler alternative rejected because |
|------|-----------|--------------------------------------|
| Two code paths (fast / telemetry) in `SendAsync` | SC-002/SC-006: no async layer or allocation when telemetry is off or unobserved | A single async path always pays one state machine |
