# Feature Specification: Complete examples

**Feature Branch**: `chore/add-spec-kit`

**Created**: 2026-11-23

**Status**: Ready

**Input**: "Complete example project showcasing every Viana.Results feature, editing the existing examples (Shared, Scalar, Swagger)."

## Clarifications

### Session 2026-11-23

- Q: Target frameworks and number of example projects? → A: .NET 10 only. (Superseded below on the number
  of projects.)
- Q: Project layout? → A: Follow the repository pattern, one example per integration: a Scalar host
  (native OpenAPI), a Swagger host (Swashbuckle) and a Mediators host. Results is the base used by all
  three (the shared catalog domain lives in the shared library). The Mediators host carries the full
  observability configuration (options, tracing/metrics switches, exception detail levels, OpenTelemetry
  export). The Scalar and Swagger hosts do not use the mediator.

## Project layout

| Project | Role |
|---------|------|
| `Viana.Results.Examples.Shared` (kept) | Catalog domain, in-memory store, controllers and minimal endpoints used by the Scalar and Swagger hosts; weather forecast samples replaced |
| `Viana.Results.Scalar` (kept) | Native OpenAPI + Scalar UI over the shared endpoints |
| `Viana.Results.Swagger` (kept) | Swashbuckle + Swagger UI over the same shared endpoints |
| `Viana.Results.Mediators.Example` (new) | Catalog use cases as mediator requests/handlers, all mediator options and observability |

All four target .NET 10 only and are never packed.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Learn the core result model from runnable code (Priority: P1)

A developer evaluating the library opens the examples and finds, in one small realistic domain (an in-memory
product catalog), every way to produce a result: success with and without a value, implicit conversion from
a value, each built-in problem (bad request, validation, unauthorized, forbidden, not found, conflict,
business rule violated, generic failure) and a custom problem built with the problem detail builder
(description, detail, extensions).

**Why this priority**: The core model is what every other package builds on; without it the rest is noise.

**Independent Test**: Run either example host and call the catalog endpoints from the provided request
files; each documented status (200, 201, 204, 400, 404, 409, 422/validation, 500, 503) is reachable.

**Acceptance Scenarios**:

1. **Given** an empty catalog, **When** a product is created with valid data, **Then** the response is 201
   with the product and its location.
2. **Given** an existing product SKU, **When** another product with the same SKU is created, **Then** the
   response is a 409 problem.
3. **Given** invalid input, **When** it is submitted, **Then** the response is a validation problem listing
   each invalid field.
4. **Given** a missing id, **When** it is fetched, updated or deleted, **Then** the response is a 404 problem.
5. **Given** an existing product, **When** it is deleted, **Then** the response is 204.

---

### User Story 2 - Same use cases through controllers and minimal APIs (Priority: P1)

The developer sees the same catalog exposed twice: through MVC controllers and through minimal API
endpoints, both returning results directly, so they can compare styles. Controllers built on the classic
MVC result conversion (interop package) are shown side by side with the native filter.

**Why this priority**: Hosting style is the first decision a consumer makes.

**Independent Test**: Every catalog operation has a controller route and a minimal API route; the request
files call both and responses are identical in status and body shape.

**Acceptance Scenarios**:

1. **Given** the same request, **When** sent to the controller route and to the minimal route, **Then** the
   status and the problem body are equivalent.
2. **Given** the interop controller, **When** it returns a result, **Then** the response follows the same
   rules as the native path.

---

### User Story 3 - Accurate API documentation in both UIs (Priority: P2)

The developer opens Scalar (native OpenAPI) and Swagger UI (Swashbuckle) and sees, for every catalog
operation, the success schema and each problem response with examples, including the documented problem
type that matches what the runtime sends, plus custom registered examples.

**Independent Test**: For every operation, each status returned at runtime appears in the document of both
hosts with a schema and an example.

**Acceptance Scenarios**:

1. **Given** an operation that can return 404 and 409, **When** the document is opened, **Then** both
   responses are listed with problem examples whose type matches the runtime value.

---

### User Story 4 - Mediator with options, errors and telemetry (Priority: P2)

Catalog operations go through mediator requests and handlers that return results. The host shows mediator
options (handler lifetime, tracing and metrics switches, exception detail level), a dispatch with no
handler, a handler that throws (500), a dependency unavailable (503), and request cancellation. Traces and
metrics are exported to the console so the developer can see the span, its tags and the duration metric.

**Independent Test**: Call each mediator scenario from the request files and observe the expected status and
the console telemetry output (span name, tags, metric).

**Acceptance Scenarios**:

1. **Given** telemetry enabled, **When** a catalog request is sent, **Then** the console shows a mediator span
   and a duration measurement with the request type and status.
2. **Given** a request with no registered handler, **When** it is sent, **Then** the response is a 500
   problem and the span is marked as an error.
3. **Given** a cancelled request, **When** it is aborted, **Then** the span is not marked as an error.
4. **Given** the exception detail level set to type only, **When** a handler throws, **Then** the exported
   exception has no message or stack trace.

---

### User Story 5 - Guided tour (Priority: P3)

A README for the examples maps each feature to the file and endpoint that demonstrates it, explains how to
run each host, and how to toggle telemetry options.

**Independent Test**: Every feature listed in the README points to an existing file and a request in the
request files.

### Edge Cases

- Telemetry switches turned off: the host still works and nothing is exported for the disabled kind.
- Request files are run against either host (only the base address differs).
- The catalog is in memory and resets on restart; concurrent creates with the same SKU yield one 201 and one 409.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The examples MUST follow the layout in "Project layout": the shared library and the Scalar and
  Swagger hosts are kept and updated, a Mediators example host is added, all targeting .NET 10 only. The
  weather forecast samples are replaced by the catalog domain.
- **FR-002**: The shared library MUST contain an in-memory product catalog with list, get, create, update and
  delete operations.
- **FR-003**: The examples MUST demonstrate every public result factory of the core package and the problem
  detail builder (description, detail and extensions), plus implicit value-to-result conversion.
- **FR-004**: Every catalog operation MUST be exposed through MVC controllers (native filter) and through
  minimal API endpoints; at least one controller MUST use the MVC interop package.
- **FR-005**: The endpoints MUST, between them, return 200, 201, 204, 400, 404, 409, a validation problem, 500
  and 503.
- **FR-006**: Both hosts MUST document every operation with success and problem responses, examples, the
  native problem type and at least one custom registered example.
- **FR-007**: In the Mediators example, catalog operations MUST be dispatched through the mediator and the host
  MUST configure mediator options explicitly (handler lifetime, tracing, metrics, exception detail level) with comments on each.
- **FR-008**: The examples MUST include a missing-handler scenario, a throwing handler, an unavailable
  dependency (503) and a cancellable long-running request.
- **FR-009**: Mediator traces and metrics MUST be exported to the console using the published diagnostic
  names, enabled by default in development.
- **FR-010**: Request files MUST exercise every endpoint (both styles) and every scenario above.
- **FR-011**: A README in the examples folder MUST map each feature to its file and request, and explain how to
  run and configure each host.
- **FR-012**: The solution MUST build with 0 warnings; the example projects MUST NOT be packed or published.
- **FR-013**: No documentation or code comment MAY name another mediator library.

### Key Entities

- **Product**: id, SKU (unique), name, price (> 0), stock (≥ 0).
- **Catalog**: in-memory store of products, thread safe, reset on restart.

## Success Criteria *(mandatory)*

- **SC-001**: 100% of the core public result factories and builder methods appear in at least one example.
- **SC-002**: Every listed status (9) is reachable from the request files on both hosts.
- **SC-003**: Every runtime status of every operation is documented in both API documents (0 gaps).
- **SC-004**: A new developer can run a host and see mediator telemetry in the console in under 5 minutes
  following the README.
- **SC-005**: Full solution build: 0 warnings; existing test suite unaffected.

## Assumptions

- The core package has no functional composition helpers (match/map); none are added here.
- Library packages gain no new dependency.
- .NET 10 only (no multi-targeting). "Both hosts" means the Scalar and Swagger hosts; the Mediators example
  also exposes its endpoints with native OpenAPI so it can be explored, but its focus is the mediator.
- OpenTelemetry packages are added to the Mediators example only.
- The 503 is simulated by a toggleable fake dependency; the 500 by a handler that throws on purpose.
- Validation problems use the status the core package already assigns to validation.
