# Feature Specification: Review leftovers (problem provider, Mvc README, duration buckets)

**Feature Branch**: `chore/add-spec-kit`

**Created**: 2026-11-25

**Status**: Ready

**Input**: "Fix the three leftovers found while building the examples: `ProducesProblemResult<P>()` with a
`ProblemResult` provider is silently ignored; the Mvc README shows a `ToActionResult()` that the package does
not have; the mediator duration histogram (seconds) uses millisecond-style default buckets."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - A ProblemResult example provider documents its problem on minimal APIs (Priority: P1)

A developer reuses the same example class on a controller (`[ResponseExample(typeof(P))]`) and on a minimal API
(`ProducesProblemResult<P>()`). The class returns a `ProblemResult` (`IExampleProvider<ProblemResult>`). Today
the controller documents it, but the minimal API silently drops it. Both must document the problem response,
with its status, schema and example.

**Independent Test**: A minimal API with `ProducesProblemResult<P>()`, where P returns a 503 `ProblemResult`,
lists a 503 `application/problem+json` response with P's example, in both generators.

**Acceptance Scenarios**:

1. **Given** `ProducesProblemResult<P>()` and P returning `ProblemResult` with status 503, **Then** the document
   lists 503 with the problem schema and P's example (title, detail, type, extensions).
2. **Given** `ProducesResult<P>()` (the success overload) with a `ProblemResult` provider, **Then** it is
   documented as the same problem response. A problem is a problem, whichever overload declares it.
3. **Given** a provider returning something that is neither a Viana result nor a `ProblemResult`, on
   `ProducesProblemResult`, **Then** document generation fails with a clear message naming the endpoint and the
   provider. It is never silently dropped.
4. **Given** the existing `IExampleProvider<Result>` providers, **Then** nothing changes.

---

### User Story 2 - The Mvc README matches the package (Priority: P2)

A developer following the Viana.Results.Mvc README writes code that compiles. The README shows the real API:
`new VianaResultAction(result)` and the `AddVianaResultFilter()` filter. It does not show `ToActionResult()`,
which belongs to Viana.Results.AspNetCore.

**Independent Test**: Every code block in the Mvc README compiles against the Mvc package alone (verified by a
test project file that mirrors the snippets).

---

### User Story 3 - Duration histogram buckets fit seconds (Priority: P2)

An operator collecting `viana.mediator.request.duration` with OpenTelemetry gets buckets that fit a
seconds-based duration (the OpenTelemetry HTTP duration recommendation: 0.005 … 10 s). Today every dispatch
lands in the first bucket.

**Independent Test**: On .NET 9+ the instrument publishes bucket advice. A `MeterListener` reads
`Instrument.Advice` (or the OpenTelemetry SDK exports the advised boundaries) and finds the expected list. On
older targets the README shows the one-line OpenTelemetry View that sets the same buckets.

**Acceptance Scenarios**:

1. **Given** net10.0 (or any .NET 9+ target), **Then** the histogram carries advice with the boundaries
   `[0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]`.
2. **Given** net6.0 and net8.0, **Then** the behavior is unchanged (the advice API does not exist there), and
   the README documents the View that sets the same boundaries.
3. **Given** the console exporter of the Mediators example, **Then** a dispatch of a few milliseconds falls in
   the 0.005 or 0.01 bucket, not in the first default bucket.

### Edge Cases

- A `ProblemResult` provider whose status is 2xx: documented under that status as a problem, the same as on
  controllers.
- An explicit `status:` that differs from the provider's problem status: an error at document generation,
  matching the existing rule for result providers.
- An application that already configures a View for the instrument: its View wins over the advice (standard
  OpenTelemetry precedence).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Minimal API declarations (`ProducesProblemResult` and `ProducesResult`) MUST accept
  `IExampleProvider<ProblemResult>` providers and document them as problem responses (status, RFC 9457 schema,
  example, native type rule). Inline overloads keep taking an `IResult`. A `ProblemResult` is passed inline as
  `new Result(problem)` or through `ProblemBuilder.BuildResult()`, so no new API is added.
- **FR-002**: A provider value that is neither `IResult` nor `ProblemResult` on a problem declaration MUST make
  document generation fail with a message naming the endpoint and the provider type.
- **FR-003**: Controllers (`[ResponseExample]`) and minimal APIs MUST document the same provider identically.
- **FR-004**: The Viana.Results.Mvc README MUST show only APIs that exist in that package, and MUST point to
  Viana.Results.AspNetCore for `ToActionResult()`.
- **FR-005**: On .NET 9+ targets, the duration histogram MUST be created with bucket advice
  `[0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]`. The boundaries MUST be exposed as
  a public constant array in `MediatorDiagnostics`, so apps can reuse them in a View.
- **FR-006**: Other targets MUST keep compiling with no new dependency. The README documents the View.
- **FR-007**: Release notes MUST be updated (no version change: nothing is published).

### Key Entities

- **Problem example provider**: a class implementing `IExampleProvider<ProblemResult>`.
- **Duration bucket boundaries**: the list in FR-005.

## Success Criteria *(mandatory)*

- **SC-001**: 0 provider kinds that a controller documents are dropped on minimal APIs.
- **SC-002**: 100% of the Mvc README snippets compile.
- **SC-003**: On net10.0, a typical dispatch (under 10 ms) is reported in a bucket at or below 0.01 s.
- **SC-004**: Full build with 0 warnings; all tests pass.

## Assumptions

- The examples keep their current providers. `ServiceUnavailableExample` was turned into an
  `IExampleProvider<Result>` as a workaround. It may go back to `ProblemResult` to show that both kinds work.
- The mediator's net9+ code path uses the net10.0 target, the only target that has `InstrumentAdvice`.
- No other mediator library is named in any document.
