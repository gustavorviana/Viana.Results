# Feature Specification: Mediators Review (performance, observability, security, usability)

**Feature Branch**: `chore/add-spec-kit` (shared branch; no dedicated feature branch)

**Created**: 2026-11-23

**Status**: Draft

**Input**: User description: "Mediators review (Viana.Results.Mediators, hot-path performance,
observability, security, usability). Fix the dispatch hot path (per-instance cache, closure, reflection per
call, nested async, telemetry cost without listener), telemetry bugs (handler tag, status tag, missing error,
undocumented source), assembly scanning robustness, handler-not-found message, wrong handler for requests
with two result types, null task, duplicate registrations, README errors. Decisions: benchmarks with
before/after baseline; keep Task; AOT later; cancellation is not an error; Error status for 5xx results;
prefixed OpenTelemetry tag names; metrics; netstandard2.0 telemetry out of scope; option to hide/mask
exception details; duplicate handlers are a registration error; derived requests not resolved to base
handlers; configurable handler lifetime; pipeline behaviors out of scope; keep Handle naming."

## Clarifications

### Session 2026-11-23 (decided during the package review)

- Q: Benchmarks? → A: Yes, a dispatch benchmark with a baseline before and after the change.
- Q: Switch handler/mediator signatures to a value-task style? → A: No, keep the current signatures.
- Q: Native AOT / trimming? → A: Later, after these fixes (separate spec).
- Q: Telemetry on netstandard2.0? → A: Out of scope; to be discussed later.
- Q: Cancellation on the span? → A: Not an error.
- Q: Span status for a failed result? → A: Error for 5xx problems, Ok for 4xx (client errors are expected
  outcomes).
- Q: Tag names? → A: OpenTelemetry style with the `viana.mediator.` prefix; on runtimes that offer a
  native exception recording API, use it.
- Q: Metrics? → A: Yes: dispatch count and duration by request type and outcome status.
- Q: Exception details in telemetry? → A: Configurable: record (default), mask, or omit message/stack.
- Q: Two handlers for the same request? → A: Error at registration.
- Q: Derived request types? → A: Not resolved to base-type handlers; documented.
- Q: Handler lifetime? → A: Configurable through the registration options (default stays scoped). **Superseded 2026-11-24**: the option is removed before 1.1.0 ships; handlers are always registered scoped, and a handler registered by hand before `AddMediator` keeps its own lifetime.
- Q: Pipeline behaviors? → A: Out of scope (future spec).
- Q: Rename `Handle`? → A: No.
- Q: Can telemetry be disabled? → A: Yes, with two independent switches (tracing, metrics), both enabled by
  default (no cost without a listener). Disabled means the telemetry source/meter are not even consulted.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Fast, correct dispatch (Priority: P1)

A developer sends requests through the mediator in a high-traffic application and expects the dispatch
cost to be close to calling the handler directly, and always to reach the right handler.

**Why this priority**: the package aims to be optimized like the core; the current dispatch repeats
type-discovery work on every request and can pick the wrong handler.

**Independent Test**: dispatch benchmark before/after; tests for repeated dispatch in new scopes, requests
with two result types, null task and handler exceptions.

**Acceptance Scenarios**:

1. **Given** a request already dispatched once, **When** it is dispatched again (even from a new scope),
   **Then** no type-discovery work is repeated and per-call allocations are lower than the baseline.
2. **Given** a request implementing two request contracts with different result types, **When** each is
   sent, **Then** each reaches its own handler.
3. **Given** a handler that throws, **When** the request is sent, **Then** the original exception (type,
   message, stack) reaches the caller.
4. **Given** a handler returning no task, **When** the request is sent, **Then** a clear error names the
   handler and the request.
5. **Given** no handler for a request, **When** it is sent, **Then** the error names the **request** type
   and exposes it as a property.

---

### User Story 2 - Useful, cheap telemetry (Priority: P2)

An operator collects traces and metrics from the mediator and expects accurate, conventional data that
costs nothing when nobody listens.

**Why this priority**: current tags are wrong (object instead of name, contract instead of handler,
inconsistent names) and failures can end without error status.

**Independent Test**: tests with an activity listener and a metrics listener; benchmark without listener.

**Acceptance Scenarios**:

1. **Given** a listener, **When** a request is sent, **Then** the span carries `viana.mediator.*` tags with
   request type, result type, handler implementation type, success flag and status, as text or numbers.
2. **Given** a handler-not-found or a handler exception, **When** the request is sent, **Then** the span
   ends with error status and the exception is recorded according to the configured detail level.
3. **Given** a cancelled request, **When** it ends, **Then** the span is not marked as error.
4. **Given** a result with a 5xx problem, **Then** the span status is Error; with a 4xx problem, Ok.
5. **Given** a metrics listener, **When** requests are sent, **Then** a duration is recorded per request
   type and outcome status; the request count is the count of those measurements.
6. **Given** no listener, **When** requests are sent, **Then** no telemetry data is built.
7. **Given** the documentation, **Then** the telemetry source and meter names are documented and exposed
   as constants.

---

### User Story 3 - Safe, predictable registration (Priority: P3)

A developer registers handlers from assemblies and expects registration to be robust, idempotent and to
fail fast on ambiguous setups.

**Why this priority**: scanning can crash on partially loadable assemblies, repeated registration
duplicates services, and duplicate handlers are resolved silently.

**Independent Test**: registration tests (repeated calls, duplicate handlers, abstract/open generic types,
partially loadable assembly, lifetime option).

**Acceptance Scenarios**:

1. **Given** registration called more than once (same or different assemblies), **Then** the mediator and
   each handler are registered once.
2. **Given** two handlers for the same request and result, **Then** registration fails with an error
   naming both handlers.
3. **Given** an assembly with types that cannot be loaded, **Then** registration continues with the
   loadable types.
4. **Given** open generic or abstract handler types, **Then** they are skipped.
5. **Given** a handler registered by hand (any lifetime) before `AddMediator`, **Then** the scan keeps that registration; scanned handlers are scoped.
6. **Given** a derived request with a handler only for its base type, **Then** sending it reports
   handler-not-found (documented behavior).

---

### User Story 4 - Safe exception details in telemetry (Priority: P3)

A security-conscious team keeps exception messages and stack traces (which may contain sensitive data)
out of the telemetry backend.

**Independent Test**: listener tests for each detail level.

**Acceptance Scenarios**:

1. **Given** the default level, **Then** type, message and stack are recorded.
2. **Given** the masked level, **Then** only the exception type is recorded (no message or stack, also
   in the span status description).
3. **Given** the omitted level, **Then** no exception event is recorded; the span still ends with error.

---

### User Story 5 - Turn telemetry off (Priority: P2)

A team that does not want mediator telemetry (or wants only metrics, or only traces) turns each kind off in
the registration options.

**Independent Test**: listener tests with each switch off; benchmark with both off.

**Acceptance Scenarios**:

1. **Given** default options, **Then** tracing and metrics are enabled.
2. **Given** tracing off and an active trace listener, **When** a request is sent, **Then** no span is
   created by the mediator; metrics still recorded.
3. **Given** metrics off and an active metrics listener, **Then** no measurement is recorded; spans still
   created.
4. **Given** both off, **Then** dispatch cost equals dispatch without any telemetry.

### Edge Cases

- Null request → argument error, delivered through the returned task like every other dispatch error
  (unchanged: no error is thrown at call time).
- Registration called several times with different options → the first options win; documented.
- Request whose runtime type differs from the declared type → resolved by runtime type plus result type.
- Handler lifetime that captures scoped services (singleton) → standard container validation applies;
  documented.
- Concurrent first dispatch of the same request type → one consistent cached entry.
- Runtimes without the native exception recording facility → equivalent manual event.
- netstandard2.0 consumers → dispatch fixes apply; no telemetry (unchanged, out of scope).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Dispatch information MUST be cached process-wide by request type **and** result type, built
  once, without per-call closures.
- **FR-002**: After the first dispatch of a request type, dispatch MUST NOT use reflection invocation;
  handler exceptions MUST propagate unchanged.
- **FR-003**: Dispatch MUST NOT add avoidable asynchronous layers or per-call allocations beyond the
  handler's own work; telemetry data MUST only be built when a listener requests it.
- **FR-004**: A handler returning no task MUST fail with a clear error naming handler and request.
- **FR-005**: The handler-not-found error MUST name the request type (and result type) and expose the
  request type as a property; the span MUST end with error status.
- **FR-006**: Span tags MUST use the `viana.mediator.` prefix (request type, result type, handler
  implementation type, success, status), as text or numbers.
- **FR-007**: Cancellation MUST NOT mark the span as error; other exceptions MUST; 5xx problem results
  MUST set Error, 4xx MUST set Ok.
- **FR-008**: Exceptions MUST be recorded with the native recording facility where available, otherwise
  with an equivalent event, honoring the detail level option (record, mask, omit).
- **FR-009**: Metrics MUST record dispatch duration by request type and outcome status; the dispatch count
  is the count of duration measurements (no separate counter).
- **FR-010**: Telemetry source and meter names MUST be public constants and documented.
- **FR-011**: Registration MUST be idempotent, skip abstract and open generic types, tolerate partially
  loadable assemblies, and fail on duplicate handlers for the same request and result.
- **FR-012**: Registration MUST accept options (no handler lifetime option: scanned handlers are scoped, hand-registered handlers keep their lifetime): exception detail
  level (record by default) and the telemetry switches of FR-017.
- **FR-013**: Derived request types MUST NOT be resolved to base-type handlers; documented.
- **FR-014**: Public contracts (`IRequest<>`, `IHandler<,>`, `IMediator`, `Handle`, `SendAsync`) MUST keep
  their signatures.
- **FR-015**: A dispatch benchmark MUST be added with a before/after baseline.
- **FR-016**: README and release notes MUST be corrected (target frameworks, telemetry runtimes, tag names,
  ASP.NET Core sample) and MUST NOT mention other mediator libraries.

- **FR-018**: Dispatch errors (null request, handler not found, null task, synchronous handler exceptions)
  MUST surface through the returned task, never at call time (current behavior kept).
- **FR-017**: Registration options MUST offer independent tracing and metrics switches, both enabled by
  default; a disabled kind MUST NOT be consulted or produce data, even with active listeners.

### Key Entities

- **Dispatch entry**: cached per (request type, result type); knows the handler contract and invokes it.
- **Mediator options**: telemetry (tracing switch, metrics switch, exception detail level).
- **Telemetry**: span with prefixed tags; metrics (count, duration) with request type and status
  dimensions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Steady-state dispatch allocates at least 50% less than the baseline and is faster than the
  baseline in the benchmark.
- **SC-002**: Without a listener, dispatch allocates the same as with telemetry absent (no telemetry data
  built).
- **SC-003**: 100% of the bug scenarios in the acceptance tests (wrong handler, null task, not-found
  message and span, duplicates) pass.
- **SC-004**: 100% of span tags and metrics match the documented names in listener tests.
- **SC-006**: With tracing and metrics off, dispatch time and allocations match a dispatch without
  telemetry in the benchmark (within noise).
- **SC-005**: 0 signature changes in the public contracts of FR-014; existing tests keep passing.

## Assumptions

- Version: the tag renames and the duplicate-handler error change observable behavior; treated as
  corrections (the current names did not match the documentation) in a minor release 1.1.0 unless planning
  finds a stronger reason for a major.
- Metrics and native telemetry are available on the package's .NET targets (net6.0, net8.0, net10.0); the
  native exception recording facility only on runtimes that provide it.
- Pipeline behaviors, value-task signatures, Native AOT/source generation and netstandard2.0 telemetry are
  out of scope.
- The earlier review findings I2–I7 are covered by FR-001, FR-003, FR-004, FR-005 and FR-006.
