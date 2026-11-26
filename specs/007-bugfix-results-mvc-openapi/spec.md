# Feature Specification: Bug-fix Sweep — Results, Mvc and OpenApi

**Feature Branch**: `chore/add-spec-kit` (shared branch; no dedicated feature branch)

**Created**: 2026-11-22

**Status**: Draft

**Input**: Code review of 2026-11-22 (public and internal code) of `Viana.Results`, `Viana.Results.Mvc`,
`Viana.Results.OpenApi` and `Viana.Results.OpenApi.Swashbuckle`. Mediators is out of scope (dedicated
future spec). Item IDs (I*, MV*, B*, M*) refer to that review.

## Clarifications

### Session 2026-11-22

- Q: Mediators findings? → A: Out of scope; a dedicated Mediators spec comes later.
- Q: Fix the old Mvc package even though Viana.Results.AspNetCore replaces it? → A: Yes. AspNetCore
  supports only net8.0/net10.0, so net6.0 users stay on Mvc. Mvc is fixed here. Its deprecation is
  not planned in this round (out of scope).
- Q: Versions? → A: I1 and I8 ship in the unreleased `Viana.Results` 3.0.0; MV1 as a MINOR bump of
  `Viana.Results.Mvc`, with release notes.
- B3 (action vs controller precedence in OpenAPI) stays in feature 006.
- Q: Extension member names differing only by casing (M5)? → A: Case-insensitive everywhere (core
  problem extensions, builder, JSON reading and OpenAPI schema). When the same name appears twice the
  last value wins and the casing of the first occurrence is kept (new item I10). No warning or
  exception; no naming policy applied (runtime writes keys as stored).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Response bodies match what the result describes (Priority: P1)

As an API developer, I want the response body produced from a result to reflect it exactly, so that
API consumers never receive invented content or lose an error.

**Why this priority**: These bugs change what clients receive (core, used by every integration).

**Independent Test**: Produce the response body for a success with a null value, for a failure with
status 204, and for a typed result whose value is a custom value-type DTO; compare to the expected
bodies.

**Acceptance Scenarios**:

1. **Given** a successful typed result whose value is null, **When** its response body is produced,
   **Then** the body represents null (not an empty object). (I1)
2. **Given** a typed result whose value is a custom structured value type (e.g. a record struct with
   properties), **When** its body is produced, **Then** it has the same shape as for an equivalent
   reference type (unwrapped). Simple values (numbers, booleans, text, dates, identifiers, enums) keep
   today's shape. (I8)
3. **Given** a failed result with status 204, **When** its body is produced, **Then** the problem is
   not silently dropped (the problem body is produced). (MV3)
4. **Given** a problem without extension members, **When** a consumer tries to modify its extension
   members, **Then** it cannot (same immutability as problems with members). (I9)
5. **Given** extension members "errorCode" = "E1" then "ErrorCode" = "E2" (via builder, factory or a
   read document), **When** the problem is built, **Then** it has a single member named "errorCode" with
   value "E2", and lookups ignore casing. (I10)

---

### User Story 2 - The old MVC package follows RFC 9457 and is efficient (Priority: P1)

As an API developer still on the MVC package (e.g. on net6.0), I want error responses with the
standard problem media type, efficient writing and a safe registration.

**Why this priority**: The content type is an RFC 9457 violation (constitution Principle IV).

**Independent Test**: Execute a failed result through the MVC package; the response content type is
the problem media type. Register the global option twice; the conversion runs once per response.

**Acceptance Scenarios**:

1. **Given** a failed result, **When** it is written by the MVC package, **Then** the content type is
   `application/problem+json`; successes keep `application/json`. (MV1)
2. **Given** any result with a body, **When** it is written, **Then** the body is streamed to the
   response without building an intermediate full copy, and writing stops when the request is aborted.
   (MV2)
3. **Given** the global option registered twice, **When** a request runs, **Then** the result is
   converted once. (MV4)

---

### User Story 3 - OpenAPI documentation is valid and accurate (Priority: P1)

As an API developer generating OpenAPI documents, I want the generated problem examples, titles and
authorization responses to be valid and match the real behavior.

**Why this priority**: Invalid examples and wrong titles mislead API consumers and client generators.

**Independent Test**: Generate the document for endpoints with problem annotations (no explicit
example), with `[AllowAnonymous]`, with `ValueTask` return types, and check examples, titles and
responses.

**Acceptance Scenarios**:

1. **Given** a problem annotation without explicit example, **When** the document is generated,
   **Then** the example is a JSON object, numbers and booleans are not quoted, and text with quotes is
   valid JSON. (B1)
2. **Given** a problem annotation without title, **When** the document is generated, **Then** the
   documented title equals the runtime default title (status reason phrase, feature 002). (B2)
3. **Given** an action returning a result wrapped in `ValueTask`, **When** the document is generated,
   **Then** it is treated like the `Task` equivalent (problem annotations applied, empty body for bare
   results). (B4)
4. **Given** an authorized controller with an `[AllowAnonymous]` action, **When** the document is
   generated, **Then** no 401/403 responses are added to that action; **given** a minimal API endpoint
   requiring authorization, **then** 401/403 are added. (B5)

---

### User Story 4 - OpenAPI package API is safe to configure and evolve (Priority: P2)

As a developer configuring the OpenAPI packages, I want configuration that composes, clear errors and
a public surface limited to what I should use.

**Why this priority**: Robustness and long-term maintainability; no wrong output today.

**Independent Test**: Register examples twice; both registrations apply. Use an example class with
constructor dependencies not registered in DI; a clear error names the class. The internal helpers are
not part of the public API.

**Acceptance Scenarios**:

1. **Given** example registration called twice, **When** the document is generated, **Then** examples
   from both calls are applied. (M2)
2. **Given** an example class whose constructor needs services, **When** the services are registered,
   **Then** it is created with them; **when** they are not, **then** the error names the class and the
   missing dependency. (M3)
3. **Given** two schema members whose names differ only by casing, **When** the schema is built,
   **Then** it documents a single member with the casing of the first occurrence and the last value as
   example, matching the runtime (I10). (M5)
4. **Given** a response with examples, **When** the Swashbuckle unwrap step runs, **Then** existing
   examples and media-type settings are preserved. (M6)
5. **Given** the OpenAPI package, **When** its public API is listed, **Then** the internal helpers (processors,
   resolver, resolved example, field schema, schema builder) are not part of it; the generated documents
   are unchanged. (M1)

### Edge Cases

- **Null value for a simple-value result** (e.g. nullable number): keeps today's shape.
- **Paged results**: unaffected by I8 (keep metadata wrapper).
- **Response already started / aborted**: writing stops without throwing to the app pipeline (MV2).
- **Global option registered once**: unchanged behavior (MV4).
- **Swashbuckle and native pipelines**: every OpenAPI fix applies to both.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001** (I1): A successful result with a null value MUST produce a null body, not an empty object.
- **FR-002** (I8): Only simple values (numbers, booleans, text, characters, dates/times, durations,
  identifiers, URIs, versions, enums and their nullable forms) MUST be treated as simple; other value
  types MUST be unwrapped like reference types. OpenAPI documentation MUST follow the same rule.
- **FR-003** (MV3): A failed result MUST produce its problem body regardless of its status.
- **FR-004** (I9): Problem extension members MUST be immutable whether empty or not.
- **FR-005** (MV1): The MVC package MUST write failures with `application/problem+json`.
- **FR-006** (MV2): The MVC package MUST stream bodies to the response and honor request abortion.
- **FR-007** (MV4): Registering the MVC global option more than once MUST NOT convert results more
  than once.
- **FR-008** (B1): Generated problem examples MUST be JSON objects with correctly typed and escaped values.
- **FR-009** (B2): Documented default titles MUST equal the runtime default titles.
- **FR-010** (B4): `ValueTask`-wrapped return types MUST be handled like `Task`-wrapped ones.
- **FR-011** (B5): Authorization responses MUST honor anonymous access and minimal API authorization
  requirements.
- **FR-012** (M1): Internal helper types of the OpenAPI package MUST be internal. They were public in
  `Viana.Results.OpenApi` 1.0.0, which is published (`main`), so this is a breaking change: the package goes to
  2.0.0 with a "Breaking changes" release-note entry. Decision 2026-11-26: no `[Obsolete]` transition period.
- **FR-013** (M2): Example registration MUST compose across multiple calls.
- **FR-014** (M3): Example classes MUST be created with their constructor dependencies when available,
  with a clear error otherwise.
- **FR-015** (M5): OpenAPI schema members MUST follow the runtime rule of FR-019 (case-insensitive,
  first casing kept, last value wins), without naming policy, warning or exception.
- **FR-016** (M6): The Swashbuckle unwrap step MUST preserve existing media-type examples and settings.
- **FR-017**: Every fix MUST be covered by a test that fails before the fix (constitution Principle III).
- **FR-019** (I10): Problem extension member names MUST be case-insensitive in the problem, the builder
  and JSON reading: when the same name appears more than once, the last value wins and the casing of the
  first occurrence is kept.
- **FR-018**: Release notes MUST describe the behavior changes: core 3.0.0 (I1, I8, MV3, I9, I10), Mvc MINOR
  (MV1, MV2, MV4), OpenApi/Swashbuckle (B1, B2, B4, B5, M1–M6) within their unreleased 1.1.0 / 1.1.2 or
  the next MINOR if already released.

### Key Entities

- **Result**: success (optional value) or failure (problem) with a status.
- **Problem**: RFC 9457 error with extension members.
- **OpenAPI document**: generated documentation of responses, schemas and examples.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 0 responses with invented content (null value → null body) and 0 dropped problems in tests.
- **SC-002**: 100% of failure responses written by the MVC package use the problem media type.
- **SC-003**: 100% of generated problem examples parse as JSON objects with correctly typed values.
- **SC-004**: Documented and runtime default titles match for 100% of known statuses.
- **SC-005**: 0 authorization responses documented on anonymous actions; 100% on authorized minimal
  API endpoints.
- **SC-006**: MVC writing allocates less than today on the benchmark (Principle V).
- **SC-007**: All 17 review items (incl. I10) listed above have a passing regression test.

## Assumptions

- Mediators is excluded; its findings are kept for a dedicated spec.
- B3 is handled in feature 006; Mvc deprecation is out of scope; minimal API documentation helpers
  in feature 008.
- The old MVC package keeps its targets (net6.0, net8.0, net10.0).
- Work happens on the shared branch `chore/add-spec-kit`.
