# Research: Mediators Review

## R1 – Dispatch cache and invocation

- **Decision**: `internal static class DispatchCache<TResult>` holding
  `ConcurrentDictionary<Type, HandlerWrapper<TResult>>` keyed by `request.GetType()`. The factory is a
  `static` lambda creating `HandlerWrapper<TRequest, TResult>` via `MakeGenericType` + `Activator` once per
  (request type, result type). The wrapper resolves `IHandler<TRequest, TResult>` from the provider and
  calls `Handle` directly (typed call, no `MethodInfo.Invoke`, exceptions propagate untouched).
- **Rationale**: the generic class makes the result type part of the key without a tuple (fixes the
  two-`IRequest<>` bug), is process-wide (fixes the scoped-instance cache), and the static lambda removes
  the per-call closure. This is the common, proven mediator dispatch design.
- **Alternatives**: tuple key `(Type, Type)` in one dictionary (extra hashing, cast); compiled expression
  delegates (more complex, AOT-hostile, same steady-state cost); source generator (AOT spec, later).

## R2 – Fast path vs telemetry path

- **Decision**: `SendAsync` is **not** `async`. If tracing and metrics are both inactive for this call
  (switch off, or `ActivitySource.HasListeners()` false and `histogram.Enabled` false) it returns the
  wrapper task directly. Otherwise it calls an `async` local path that starts the activity, awaits, sets
  tags/status and records the duration.
- **Rationale**: removes the two nested state machines of today (P4) and makes "unobserved" equal to "no
  telemetry" (SC-002/SC-006). `IsAllDataRequested` gates tag building when an activity exists but is
  only propagated (P5).
- **Error semantics (kept)**: today every error arrives as a faulted task (the method is `async`). The fast
  path keeps that: the dispatch body runs in `try/catch` and returns `Task.FromException<TResult>(ex)`
  (no state machine), so `var t = SendAsync(r); ...; await t` and `Task.WhenAll` behave as before (FR-018).
- **Alternatives**: always-async with cheap checks (still one state machine per call); throwing at call
  time (behavior change, rejected).

## R3 – Tags, names and status

- **Decision**: source and meter name `Viana.Results.Mediators` (unchanged source name, now public in
  `MediatorDiagnostics.SourceName`/`MeterName`). Span name `Mediator.Send {RequestType}` (Internal).
  Tags: `viana.mediator.request.type`, `viana.mediator.result.type`, `viana.mediator.handler.type`,
  `viana.mediator.result.success` (bool), `viana.mediator.result.status` (int), plus the OpenTelemetry
  standard `error.type` on failures. Type names are friendly (`Result<User>` instead of ``Result`1``),
  computed once per wrapper.
- **Status**: exception → Error; `OperationCanceledException` → status left Unset, no exception event,
  tag `viana.mediator.cancelled=true`; result with `Status >= 500` and a problem → Error (description = problem
  title); otherwise Ok.
- **Rationale**: OpenTelemetry naming (lowercase, dotted, library prefix), `error.type` is the standard
  failure attribute; 4xx are expected outcomes, mirroring how HTTP server spans treat them.

## R4 – Exception recording and detail level

- **Decision**: `ExceptionDetailLevel.Full` (default): net9+ `Activity.AddException(ex)` (type, message,
  stack per OpenTelemetry), older targets an equivalent `exception` event; status description =
  message. `TypeOnly`: event with `exception.type` only, status description = type name. `None`: no event,
  status Error with no description; `error.type` still set (type name is not sensitive).
- **Rationale**: S1 — messages/stacks can carry sensitive data; the type alone keeps the trace useful.

## R5 – Metrics

- **Decision**: one histogram `viana.mediator.request.duration` (unit `s`) with dimensions
  `viana.mediator.request.type`, `viana.mediator.result.status`, `viana.mediator.result.success` and
  `error.type` when failing. The **count** is the histogram count (no separate counter).
- **Rationale**: OpenTelemetry guidance: a duration histogram already provides the request count; one
  measurement per call instead of two keeps dispatch cheap. Satisfies FR-009 (count and duration).
- **Alternatives**: separate counter (double cost, redundant data).

## R6 – Options and switches

- **Superseded 2026-11-24**: `HandlerLifetime` removed (see tasks Phase 9); handlers are scoped, hand-registered handlers keep their lifetime.
- **Decision**: `MediatorOptions { HandlerLifetime = Scoped; Telemetry = new() }`,
  `MediatorTelemetryOptions { Tracing = true; Metrics = true; ExceptionDetails = Full }`. New overload
  `AddMediator(this IServiceCollection, Action<MediatorOptions> configure, params Assembly[] assemblies)`;
  options registered as singleton (`TryAddSingleton`). `Mediator` gains a public constructor
  `(IServiceProvider, MediatorOptions)`; the existing `(IServiceProvider)` constructor stays (defaults).
  A disabled kind is never consulted (FR-017).
- **Rationale**: additive, explicit, no `IOptions<>` dependency (would need a new package).

## R7 – Registration

- **Decision**: `TryAdd(ServiceDescriptor)` for `IMediator` (scoped) and options. Types from
  `assembly.DefinedTypes` with `ReflectionTypeLoadException` → loadable types only; skip abstract,
  interfaces and generic type definitions. For each closed `IHandler<,>`: an existing descriptor with the
  **same** implementation → skip (idempotent); a **different** implementation → `InvalidOperationException`
  naming request, result and both handlers. Handlers registered with `options.HandlerLifetime`.
  Repeated calls reuse the first options (first registration wins, documented).
- **Testability**: all public overloads delegate to internal
  `AddMediatorCore(IServiceCollection, Action<MediatorOptions>?, IEnumerable<Type> candidateTypes)`; tests
  pass explicit type lists (duplicates live in nested types never scanned by assembly-based tests).
- **Rationale**: FR-011/FR-012; fail-fast on ambiguity (U5).

## R8 – Errors

- **Decision**: `HandlerNotFoundException(Type requestType, Type resultType)` (new ctor), message
  "No handler registered for request type {Request} returning {Result}." and properties `RequestType`,
  `ResultType`; existing constructors kept (`RequestType` set from the given type). Null task →
  `InvalidOperationException("Handler {Handler} returned a null task for {Request}.")`. Derived requests:
  lookup by exact runtime type, documented (FR-013).

## R9 – Benchmark

- **Decision**: `MediatorBenchmarks`: `DirectHandler` (baseline), `Send`, `Send_WithListener`,
  `Send_TelemetryOff`. Run **before** changing `Mediator` (record `baseline/mediators-1.0.0.md`, "before"),
  then after.

## R10 – Version

- **Decision**: 1.1.0 with release notes listing the renamed tags (approved by the user, Principle VII
  rationale in plan.md).

## R11 – Test targets and internals

- **Decision**: tests/Viana.Results.Mediators.Tests targets `net8.0;net10.0` (net10 exercises the
  `Activity.AddException` path; net6.0 has no supported runtime). `InternalsVisibleTo("Viana.Results.Mediators.Tests")`
  replaces the unused `InternalsVisibleTo("Viana.Results.Mvc")`, so tests check the cached wrapper instance
  through `DispatchCache<TResult>` directly.
