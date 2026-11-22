# Feature Specification: Minimal APIs Support

**Feature Branch**: `chore/add-spec-kit` (shared branch; no dedicated feature branch)

**Created**: 2026-11-22

**Status**: Draft

**Input**: User description: "Minimal APIs support. Runtime (Viana.Results.AspNetCore): convert
Result/Result<T>/ListResult<T>/PagedResult<T> to an HTTP result for minimal API endpoints with the same
rules as the MVC part (feature 006) [...]; provide ToHttpResult() and an endpoint filter so handlers can
return Viana results directly. Documentation (Viana.Results.OpenApi, no new dependency): ProducesResult<T>()
(data type or example provider class, auto-detected), ProducesResult(value) with a real result instance,
ProducesProblemResult(...) with metadata, an example class or an inline failed result; status from the
result when an example is given (mismatch is an error). Example providers may return the real Result<T>,
ListResult or PagedResult, serialized with the runtime body rules, for minimal APIs and controllers
([ResponseExample] without status). Same per-status source precedence as feature 006 and the shared
VianaProblemDetails schema. Endpoint metadata must be read by both OpenAPI pipelines."

## Clarifications

### Session 2026-11-22 (decided while designing features 006/007)

- Q: Where do the documentation helpers live? → A: In the OpenAPI package (documentation only); no new
  dependency (minimal APIs are part of the ASP.NET Core shared framework the package already uses).
- Q: `ProducesResult<T>()` for both data types and example classes? → A: Yes, auto-detected: if `T` is an
  example provider it is used as example class, otherwise `T` is the documented data type.
- Q: Inline values? → A: Yes, for developers who do not want to create example classes:
  `ProducesResult(value)` / `ProducesProblemResult(value)` with a real result instance (minimal APIs only:
  attributes cannot hold instances).
- Q: Example class preferred style? → A: `ProducesResult<UserOkExample>()` /
  `ProducesProblemResult<UserNotFoundExample>()`; status comes from the example's result.
- Q: Async schema generation (analysis M1)? → A: Option C — the documentation processing is async end to
  end: native uses the async schema API with `await`; the Swashbuckle filters that generate schemas become
  async filters (`IOperationAsyncFilter`) and wrap the already computed schema in a completed `ValueTask`.
  No synchronous waiting anywhere. The synchronous Swashbuckle path (`ISwaggerProvider.GetSwagger`) is
  verified; if it skips async filters, a non-blocking synchronous filter is kept for it.
- Q: Body shape of result examples? → A: Same as the runtime body (value unwrapped, list as array, paged
  with metadata, failure as problem).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Return Viana results from minimal API endpoints (Priority: P1)

As an API developer using minimal APIs, I want my endpoint handlers to return Viana results and get the
same responses as with controllers (native problem details for failures, normal JSON for successes).

**Why this priority**: Without runtime support, the package does not cover minimal APIs at all.

**Independent Test**: An endpoint returning a failed result produces the same response as the equivalent
controller action (status, problem media type, members); an endpoint returning a success writes its
value with the app JSON settings.

**Acceptance Scenarios**:

1. **Given** a handler returning the explicit conversion of a failed result, **When** called, **Then** the
   response is the native problem response with the same precedence rules as feature 006 (Viana values,
   native defaults, customization last, native trace id, naming policy).
2. **Given** a handler returning a successful result, **When** called, **Then** the body is written with
   the minimal API JSON options and the result status; lists are arrays, paged results keep metadata, no
   value means no body.
3. **Given** the opt-in endpoint filter, **When** a handler returns a Viana result directly (sync or
   async), **Then** the response equals the explicit conversion; other return values are untouched.
4. **Given** the same result, **When** returned from a controller and from a minimal API endpoint,
   **Then** both responses are identical (excluding per-request values).

---

### User Story 2 - Document minimal API responses with Viana helpers (Priority: P1)

As an API developer, I want to declare on a minimal API endpoint which success and problem responses it
returns, with typed examples, and see them in the OpenAPI document.

**Why this priority**: Minimal API endpoints cannot use attributes the way controllers do; without these
helpers their documentation is incomplete.

**Independent Test**: An endpoint declared with a success example class and a problem example class
produces, in the OpenAPI document (both pipelines), a 200 response with the unwrapped value schema and
example and a 404 problem response from the example, with statuses taken from the examples.

**Acceptance Scenarios**:

1. **Given** `ProducesResult<User>()`, **When** documented, **Then** a 200 response with the `User`
   schema (unwrapped) is listed.
2. **Given** `ProducesResult<UserOkExample>()` where the class returns a successful result, **When**
   documented, **Then** the response status and example come from that result.
3. **Given** `ProducesResult(value)` / `ProducesProblemResult(value)` with an inline result, **When**
   documented, **Then** status and example come from that result.
4. **Given** `ProducesProblemResult(404, title: ..., detail: ...)`, **When** documented, **Then** a 404
   problem response with those values is listed (shared schema, problem media type).
5. **Given** an explicit status that differs from the example result's status, **When** the endpoint is
   registered or documented, **Then** a clear error names the endpoint and both statuses.
6. **Given** several problem examples for the same status, **When** documented, **Then** the schema is the
   union of their members and all examples are listed.
7. **Given** precedence across sources, **When** documented, **Then** endpoint examples > endpoint
   metadata > global example (same rule as feature 006).

---

### User Story 3 - Example classes returning real results (Priority: P2)

As an API developer, I want example classes to return the real result (success or failure, list or paged)
instead of the raw value, for minimal APIs and controllers, so the example is exactly what the API sends.

**Why this priority**: Improves fidelity and lets one example class serve runtime-like documentation;
existing raw-value providers keep working.

**Independent Test**: A controller action with `[ResponseExample(typeof(UserOkExample))]` (no status)
whose class returns a successful `Result<User>` documents a 200 response whose example is the unwrapped
user; a class returning a failed result documents a problem response.

**Acceptance Scenarios**:

1. **Given** an example class returning a successful typed result, **When** documented, **Then** the
   example is the value (unwrapped) and the status is the result status.
2. **Given** an example class returning a list or paged result, **When** documented, **Then** the example
   is an array / the paged wrapper, as at runtime.
3. **Given** an example class returning a failed result, **When** documented, **Then** it is a problem
   example for the result status.
4. **Given** `[ResponseExample]` with an explicit status that differs from the result status, **When**
   documented, **Then** a clear error is reported.
5. **Given** an existing example class returning a raw value, **When** documented, **Then** behavior is
   unchanged.

### Edge Cases

- **Example class registered in DI**: resolved from DI, as today; constructor dependencies injected.
- **Null value in a success example**: documented exactly as the runtime answers: a 204 response without
  body (MVC default and minimal APIs aligned).
- **Handler returning a non-Viana value with the filter on**: untouched.
- **Endpoint without any Viana declaration**: documentation unchanged.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The ASP.NET Core package MUST convert any Viana result to a minimal API HTTP result with the
  same failure rules as feature 006 (FR-002..FR-005, FR-003a, FR-018 there) and success rules adapted to
  minimal APIs (minimal API JSON options, result status, runtime body shapes).
- **FR-002**: An explicit conversion and an opt-in endpoint filter MUST be provided; with the filter,
  handlers may return Viana results directly (sync and async).
- **FR-003**: Controller and minimal API responses for the same result MUST be identical, excluding
  per-request values.
- **FR-004**: The OpenAPI package MUST provide endpoint extensions to document success results (data type,
  example class, inline value) and problem results (metadata, example class by generic or type, inline
  failed result), readable by both OpenAPI pipelines.
- **FR-005**: `ProducesResult<T>()` MUST treat `T` as an example class when it implements the example
  provider contract, otherwise as the documented data type.
- **FR-006**: When an example (class or inline) is given, the documented status MUST come from the
  example's result; an explicit status that differs MUST produce a clear error.
- **FR-007**: Example classes MAY return real results (typed, list, paged, failed); their examples MUST be
  serialized with the runtime body rules, for minimal APIs and controllers; `[ResponseExample]` MUST accept
  a provider without status (status taken from the result).
- **FR-008**: Per-status precedence MUST follow feature 006 (endpoint example > endpoint declaration >
  global example); problem responses MUST use the shared problem schema and media type.
- **FR-009**: Existing raw-value example classes and existing controller documentation MUST keep working.
- **FR-010**: Docs (package READMEs, examples app with a minimal API endpoint) MUST cover the new APIs.

### Key Entities

- **Endpoint documentation declaration**: success/problem response declared on a minimal API endpoint
  (type, example class, inline value, metadata).
- **Result example**: an example whose value is a real result; status and body derived from it.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For a representative set of results (success value, list, paged, no content, failures
  400/404/409/500), controller and minimal API responses are identical in 100% of cases (excluding
  per-request values).
- **SC-002**: 100% of minimal API endpoints declared with the helpers appear in the OpenAPI document with
  the declared statuses, schemas and examples, in both pipelines.
- **SC-003**: 100% of result-returning examples match the runtime body for the same result.
- **SC-004**: 0 changes in documentation output for existing controllers and raw-value examples.
- **SC-005**: A developer can document an endpoint's success and error responses with at most one line per
  response.

## Assumptions

- Targets follow the packages: ASP.NET Core package net8.0/net10.0; OpenAPI package net10.0.
- Newtonsoft is not involved in minimal APIs (they use System.Text.Json options).
- Controller and minimal API responses are identical when both JSON options (`Mvc.JsonOptions` and
  `Http.Json.JsonOptions`) are configured the same way (defaults: camelCase).
- Releases: additive features → MINOR versions of the ASP.NET Core and OpenAPI packages (1.1.0 / next
  MINOR of OpenApi and Swashbuckle), bundled with their unreleased versions when applicable.
- Work happens on the shared branch `chore/add-spec-kit`.
