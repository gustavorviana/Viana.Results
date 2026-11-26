# Tasks: Mediators Review

**Input**: Design documents from `specs/009-mediators-review/`

**Prerequisites**: plan.md, spec.md, research.md (R1–R10), data-model.md, contracts/public-api.md

**Tests**: Required (constitution Principle III). Tests first; they must fail before the implementation.

## Phase 1: Setup

- [X] T001 Change tests/Viana.Results.Mediators.Tests/Viana.Results.Mediators.Tests.csproj to `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>` and in src/Viana.Results.Mediators/AssemblyInfo.cs replace `InternalsVisibleTo("Viana.Results.Mvc")` with `InternalsVisibleTo("Viana.Results.Mediators.Tests")` (research R11); then record baseline: `dotnet build Viana.Results.slnx -c Release` (0 warnings) and `dotnet test Viana.Results.slnx -c Release` (all passing; note the count)
- [X] T002 Add a project reference to src/Viana.Results.Mediators in benchmarks/Viana.Results.Benchmarks/Viana.Results.Benchmarks.csproj and create benchmarks/Viana.Results.Benchmarks/MediatorBenchmarks.cs (`[MemoryDiagnoser]`; scoped provider with one handler; `DirectHandler` baseline calling the handler directly, `Send`, `Send_WithListener` with an `ActivityListener` sampling `AllDataAndRecorded` + a `MeterListener` on the source/meter name `Viana.Results.Mediators`); run it on the **current** code and record "before" in benchmarks/Viana.Results.Benchmarks/baseline/mediators-1.0.0.md (research R9). `Send_TelemetryOff` is added in T019 once options exist

## Phase 2: Foundational

- [X] T003 Create public options and names (contracts/public-api.md): src/Viana.Results.Mediators/MediatorOptions.cs (`HandlerLifetime` default `ServiceLifetime.Scoped`; `Telemetry` get-only, `new()`), src/Viana.Results.Mediators/MediatorTelemetryOptions.cs (`Tracing = true`, `Metrics = true`, `ExceptionDetails = ExceptionDetailLevel.Full`), src/Viana.Results.Mediators/ExceptionDetailLevel.cs (`Full`, `TypeOnly`, `None`), src/Viana.Results.Mediators/MediatorDiagnostics.cs (`SourceName`/`MeterName` = "Viana.Results.Mediators", `DurationInstrument` = "viana.mediator.request.duration", tag constants `viana.mediator.request.type`, `.result.type`, `.handler.type`, `.result.success`, `.result.status`, `viana.mediator.cancelled`), all with XML docs

## Phase 3: User Story 1 – Fast, correct dispatch (P1) 🎯 MVP

**Goal**: process-wide typed dispatch, no reflection after first call, correct handler, clear errors.

**Independent Test**: DispatchTests pass; benchmark allocations below baseline.

- [X] T004 [P] [US1] Tests in tests/Viana.Results.Mediators.Tests/DispatchTests.cs: request implementing `IRequest<Result<A>>` and `IRequest<Result<B>>` reaches each handler (no InvalidCastException); dispatch from two different scopes reuses the cache (same `DispatchCache<TResult>.Get(type)` instance, read through internals); handler exception propagates with the original type, message and stack (no TargetInvocationException); handler returning a null task → `InvalidOperationException` whose message contains handler and request names; no handler → `HandlerNotFoundException` with message naming the **request** and result type and `RequestType`/`ResultType` set; derived request with only a base handler → `HandlerNotFoundException` (FR-013); errors surface only through the task (FR-018): `SendAsync(null)`, handler-not-found, null task and a handler throwing **synchronously** (before returning a task) do not throw at call time — the returned task is faulted and `await` throws the original exception (`ArgumentNullException`, etc.); concurrent first dispatch (Parallel.For) returns correct results
- [X] T005 [US1] Create src/Viana.Results.Mediators/Dispatch/HandlerWrapper.cs: internal abstract `HandlerWrapper<TResult>` (`Task<TResult> Handle(IRequest<TResult>, IServiceProvider, CancellationToken, out string handlerTypeName)` or equivalent exposing the resolved handler type name, `RequestTypeName`, `ResultTypeName` as friendly names computed once, e.g. `Result<User>`) and sealed `HandlerWrapper<TRequest, TResult>` resolving `IHandler<TRequest, TResult>` via `GetService` (missing → `HandlerNotFoundException(typeof(TRequest), typeof(TResult))`), calling `Handle((TRequest)request, ct)` directly, null task → `InvalidOperationException("Handler {Handler} returned a null task for {Request}.")` (research R1, R8)
- [X] T006 [US1] Create src/Viana.Results.Mediators/Dispatch/DispatchCache.cs: internal static `DispatchCache<TResult>` with `ConcurrentDictionary<Type, HandlerWrapper<TResult>>` and `Get(Type requestType)` using a `static` factory (`MakeGenericType` + `Activator.CreateInstance` once per type, no closure) (research R1)
- [X] T007 [US1] Update src/Viana.Results.Mediators/HandlerNotFoundException.cs: new ctor `(Type requestType, Type resultType)` with message "No handler registered for request type {Request} returning {Result}.", properties `RequestType` and `ResultType` (`Type?`); existing ctors kept and set `RequestType` (research R8)
- [X] T008 [US1] Rewrite dispatch in src/Viana.Results.Mediators/Mediator.cs: remove the instance cache and `HandlerInfo`; `SendAsync` non-async: null check, `DispatchCache<TResult>.Get(request.GetType())`, fast path returns the wrapper task directly when no telemetry is active (temporary: always fast path until US2), with the body in `try/catch` returning `Task.FromException<TResult>(ex)` so no error is thrown at call time (FR-018, research R2); add ctor `Mediator(IServiceProvider, MediatorOptions)` keeping `Mediator(IServiceProvider)` (defaults) (`MediatorActivity`/`ActivityExtension` stay until T012) — T004 green

## Phase 4: User Story 2 – Useful, cheap telemetry (P2)

**Goal**: `viana.mediator.*` spans and duration histogram, correct status, nothing built without listener.

**Independent Test**: TelemetryTests and MetricsTests pass on net6.0/net8.0/net10.0.

- [X] T009 [P] [US2] Tests in tests/Viana.Results.Mediators.Tests/TelemetryTests.cs (`#if NET6_0_OR_GREATER`, `ActivityListener` filtered by `MediatorDiagnostics.SourceName` and by a test-unique request type): span name `Mediator.Send {RequestType}`, kind Internal; tags request/result/handler **implementation** type as friendly strings, success bool, status int; 4xx problem → Ok; 5xx problem → Error with the problem title as description; handler exception → Error + exception event (type/message/stack) + `error.type`; handler not found → Error + `error.type`; `OperationCanceledException` → status Unset, no exception event, `viana.mediator.cancelled=true`; listener with `PropagationData` → no tags built (tags absent); span is a child of the ambient activity
- [X] T010 [P] [US2] Tests in tests/Viana.Results.Mediators.Tests/MetricsTests.cs (`MeterListener` on `MediatorDiagnostics.MeterName`): one `viana.mediator.request.duration` measurement per send, unit `s`, value > 0, tags request.type/result.status/result.success; failure adds `error.type`; no measurement when no listener is enabled for the instrument
- [X] T011 [US2] Create src/Viana.Results.Mediators/Telemetry/MediatorTelemetry.cs (`#if NET6_0_OR_GREATER`): static `ActivitySource` and `Meter` (name `MediatorDiagnostics.SourceName`, version = informational version), `Histogram<double>` `viana.mediator.request.duration` unit `s`; `IsActive(options)` = (`Tracing` && `source.HasListeners()`) || (`Metrics` && `histogram.Enabled`); start activity only when `Tracing`; set tags only when `IsAllDataRequested`; finish with status rules of data-model.md ("Outcome rules" table); exception recording per R4 (`Activity.AddException` under `#if NET9_0_OR_GREATER`, else manual `exception` event); histogram record with dimensions only when `Metrics` && `histogram.Enabled` (research R2–R5)
- [X] T012 [US2] Wire telemetry in src/Viana.Results.Mediators/Mediator.cs: when `MediatorTelemetry.IsActive` → private `async` path (start, await wrapper task, tags incl. handler type name, status, duration via `Stopwatch.GetTimestamp`), else fast path; HandlerNotFound now gets Error status (fixes I6); delete src/Viana.Results.Mediators/MediatorActivity.cs and src/Viana.Results.Mediators/ActivityExtension.cs — T009/T010 green

## Phase 5: User Story 5 – Turn telemetry off (P2)

**Goal**: independent tracing/metrics switches, default on, disabled kinds never consulted.

**Independent Test**: OptionsTests pass; `Send_TelemetryOff` ≈ `DirectHandler` + dispatch.

- [X] T013 [P] [US5] Tests in tests/Viana.Results.Mediators.Tests/OptionsTests.cs: defaults (`Tracing` and `Metrics` true, `ExceptionDetails` Full, `HandlerLifetime` Scoped); tracing off + active activity listener → no span, histogram still recorded; metrics off + active meter listener → no measurement, span still created; both off → neither
- [X] T014 [US5] Make `Mediator` read `MediatorOptions` (ctor from T008) in `MediatorTelemetry.IsActive`/start/record so a disabled kind is not consulted (FR-017) — T013 green

## Phase 6: User Story 4 – Safe exception details (P3)

**Goal**: record / mask / omit exception details.

**Independent Test**: detail-level tests pass.

- [X] T015 [P] [US4] Tests in tests/Viana.Results.Mediators.Tests/TelemetryTests.cs (detail levels): `Full` → event with type, message, stack and status description = message; `TypeOnly` → event with `exception.type` only, status description = type name, no message anywhere on the span; `None` → no exception event, status Error without description, `error.type` present
- [X] T016 [US4] Implement detail levels in src/Viana.Results.Mediators/Telemetry/MediatorTelemetry.cs (research R4) — T015 green

## Phase 7: User Story 3 – Safe, predictable registration (P3)

**Goal**: idempotent registration, duplicate-handler error, robust scanning, lifetime option.

**Independent Test**: RegistrationTests pass.

- [X] T017 [P] [US3] Tests in tests/Viana.Results.Mediators.Tests/RegistrationTests.cs: `AddMediator` twice (same assembly; then two calls with different assemblies) → one `IMediator` descriptor and one descriptor per handler interface; two different handlers for the same request/result → `InvalidOperationException` naming request, result and both handlers (through internal `AddMediatorCore` with an explicit type list; duplicate handlers are nested types passed only in explicit lists, so assembly-based tests never see them); abstract and open generic handler types skipped; `HandlerLifetime = Transient`/`Singleton` honored; options overload registers `MediatorOptions` once (first wins) and the mediator uses them; `ReflectionTypeLoadException` path returns loadable types (internal `GetLoadableTypes` tested with a stub exception)
- [X] T018 [US3] Rewrite src/Viana.Results.Mediators/ServiceCollectionExtensions.cs: internal `AddMediatorCore(IServiceCollection, Action<MediatorOptions>?, IEnumerable<Type> candidateTypes)` used by every public overload (assembly overloads pass `GetLoadableTypes`); new overload `AddMediator(Action<MediatorOptions> configure, params Assembly[] assemblies)`; existing overloads delegate with default options; `TryAdd` scoped `IMediator` (factory using options) and `TryAddSingleton` options; internal `GetLoadableTypes(Assembly)` (`DefinedTypes`, catch `ReflectionTypeLoadException` → non-null `Types`); skip abstract, interface and generic type definitions; per closed `IHandler<,>`: same implementation already registered → skip, different → throw (message from T017), else add with `HandlerLifetime` (research R7) — T017 green

## Phase 8: Polish & Cross-Cutting

- [X] T019 Add `Send_TelemetryOff` (options with `Tracing = false`, `Metrics = false`) to benchmarks/Viana.Results.Benchmarks/MediatorBenchmarks.cs; run all; record "after" in benchmarks/Viana.Results.Benchmarks/baseline/mediators-1.0.0.md and check SC-001 (≥ 50% fewer allocations and faster than "before" for `Send`), SC-002 (`Send` without listener = no telemetry cost) and SC-006 (`Send_TelemetryOff` ≈ `Send` without listener); justify any regression
- [X] T020 [P] Rewrite src/Viana.Results.Mediators/README.md: real TFMs (netstandard2.0, net6.0, net8.0, net10.0), telemetry on net6.0+ only, OpenTelemetry setup with `MediatorDiagnostics.SourceName`/`MeterName`, tag and instrument table, status rules, options (`HandlerLifetime`, `Telemetry.Tracing`/`Metrics`/`ExceptionDetails`), duplicate-handler error, first `AddMediator` options win on repeated calls, errors always through the returned task, derived requests not resolved to base handlers, singleton-lifetime caveat, ASP.NET Core sample with `using Viana.Results.AspNetCore` and `ToActionResult()`; no mention of other mediator libraries
- [X] T021 [P] Version 1.1.0 and release notes in src/Viana.Results.Mediators/Viana.Results.Mediators.csproj (fixes, renamed tags old → new, new options/metrics, duplicate-handler error)
- [X] T022 Run quickstart.md steps 2–5 (build 0 warnings, full tests all TFMs, benchmark recorded, no other mediator library named in the package docs or specs)

## Phase 9: Follow-up – remove the handler lifetime option (2026-11-24)

The option adds little: handlers almost always depend on scoped services, a global Singleton breaks them,
and a handler registered by hand before `AddMediator` already keeps its own lifetime. 1.1.0 is not
published, so removing it breaks nobody.

- [X] T023 Update tests first: in tests/Viana.Results.Mediators.Tests/OptionsTests.cs drop the `HandlerLifetime` default assertion; in tests/Viana.Results.Mediators.Tests/RegistrationTests.cs replace the `HandlerLifetime = Transient`/`Singleton` cases with: scanned handlers are `Scoped`; a handler registered by hand as `Singleton` (same implementation type) or through a factory before `AddMediator` keeps its descriptor and lifetime
- [X] T024 Remove `HandlerLifetime` from src/Viana.Results.Mediators/MediatorOptions.cs and register scanned handlers with `ServiceLifetime.Scoped` in src/Viana.Results.Mediators/ServiceCollectionExtensions.cs — T023 green
- [X] T025 [P] Docs: src/Viana.Results.Mediators/README.md (drop the option, add "Overriding a handler lifetime": register it before `AddMediator`); release notes in src/Viana.Results.Mediators/Viana.Results.Mediators.csproj (no lifetime option); examples/Viana.Results.Mediators.Example/Program.cs and appsettings.json (drop `HandlerLifetime`); examples/README.md options table; specs/010-complete-examples/spec.md, tasks.md (mention of the option)
- [X] T026 Build `-c Release` (0 warnings) and run the full test suite

## Dependencies & Execution Order

- T001 → T002 (baseline **before** any code change) → T003 → US1 (T004 → T005 → T006 → T007 → T008) → US2 ((T009 ∥ T010) → T011 → T012) → US5 (T013 → T014) → US4 (T015 → T016) → US3 (T017 → T018) → T019 → (T020 ∥ T021) → T022
- US3 (registration) only depends on T003 and could run in parallel with US1/US2.

## Parallel Example

```text
US2 tests: T009 ∥ T010
US3:       T017 → T018 (parallel with US1/US2 after T003)
Polish:    T020 ∥ T021
```

## Implementation Strategy

1. Baseline first (T001–T002).
2. MVP: US1 (correct, fast dispatch).
3. US2 + US5: telemetry and its switches.
4. US4: exception detail levels.
5. US3: registration.
6. Polish: benchmark after, README, version, validation.
