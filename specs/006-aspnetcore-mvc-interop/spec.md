# Feature Specification: ASP.NET Core Package with Native ProblemDetails Interop (MVC)

**Feature Branch**: `chore/add-spec-kit` (shared branch; no dedicated feature branch)

**Created**: 2026-11-21

**Status**: Draft

**Input**: User description: "Create the Viana.Results.AspNetCore package (net8.0; net10.0) as the
successor of Viana.Results.Mvc, starting with ASP.NET Core MVC support and full interop with the
native ProblemDetails pipeline (constitution Principle IX). Converting a Result/Result<T>/
ListResult<T>/PagedResult<T> to an action result must use the framework's ProblemDetailsFactory for
failures, so native defaults (traceId, type links per status) and CustomizeProblemDetails apply.
Precedence: values explicitly set in Viana (status, title, type, detail, instance, extensions) win
over native defaults; CustomizeProblemDetails runs last and can change anything. Success results go
through normal MVC content negotiation and output formatters (including AddNewtonsoftJson). Provide a
ToActionResult() extension and an opt-in global filter so actions can return IResult directly, with
the same public surface as Viana.Results.Mvc where it makes sense. The produced problem body must be
RFC 9457 compliant and identical to what the native pipeline produces for the same values.
Viana.Results.Mvc is not changed in this feature; Minimal APIs are a separate
feature."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Failures look exactly like native problem details (Priority: P1)

As an API developer, when an action returns a failed result, I want the error response to be built by
the framework's own problem details pipeline, so that it carries the same defaults my other errors
have (trace id, type link per status) and my global problem customization applies to it too.

**Why this priority**: This is the core promise of the package (Principle IX): Viana errors and
framework errors are indistinguishable to API consumers, and app-wide customization works everywhere.

**Independent Test**: In an app with problem details customization that adds an extension member, an
action returns a 404 failure without type; the response matches what the framework returns for its
own 404 (same members, trace id, type link), plus the Viana title and the customization's member.

**Acceptance Scenarios**:

1. **Given** a failure with status 404 and no type, **When** it is returned from an action, **Then**
   the body contains the status, the Viana title, the framework's default type link for 404 and the
   request trace identifier.
2. **Given** a failure with explicit title, type, detail, instance and extension members, **When** it
   is returned, **Then** every explicit value appears unchanged in the body, even where the framework
   has its own default for that member.
3. **Given** the app customizes problem details globally (e.g. adds a member or rewrites the title),
   **When** a Viana failure is returned, **Then** the customization is applied after the Viana values
   and may change any of them.
4. **Given** the same status, title, type and detail, **When** the response is produced by Viana and by
   the framework's own problem result, **Then** both bodies have the same members and values (apart
   from per-request values such as the trace identifier).
5. **Given** a failure, **When** the response is written, **Then** its content type is the problem
   details media type.

---

### User Story 2 - Successes behave like normal MVC results (Priority: P1)

As an API developer, when an action returns a successful result, I want its payload to go through the
framework's normal response formatting, so that content negotiation and the serializer I configured
(including alternative JSON serializers) are honored.

**Why this priority**: Without it, apps using a non-default serializer or content negotiation get
inconsistent responses; it is half of the conversion.

**Independent Test**: In an app configured with an alternative JSON serializer, an action returns a
successful typed result; the response is formatted by that serializer with the status from the result.

**Acceptance Scenarios**:

1. **Given** a success with a value, **When** it is returned, **Then** the status is the result's
   status and the body is the value formatted by the app's configured formatters.
2. **Given** a success without a value (e.g. 204), **When** it is returned, **Then** no body is written.
3. **Given** a list result, **When** it is returned, **Then** the body is the list items (unwrapped),
   matching current library behavior; **given** a paged result, **then** the body keeps its paging
   metadata.
4. **Given** the client asks for a media type the app supports (e.g. XML when configured), **When** a
   success is returned, **Then** the response uses that media type.

---

### User Story 3 - Return results directly from actions (Priority: P2)

As an API developer, I want to either convert a result explicitly or register a global option once and
return results directly from my actions, so that controllers stay concise.

**Why this priority**: Ergonomics; the explicit conversion already delivers the value of US1/US2.

**Independent Test**: With the global option registered, an action whose return value is a result
produces the same response as the explicit conversion; without the option, the explicit conversion
works on its own.

**Acceptance Scenarios**:

1. **Given** no global option, **When** an action returns the explicit conversion of a result,
   **Then** the response follows US1/US2.
2. **Given** the global option is registered, **When** an action returns a result directly (sync or
   async), **Then** the response is identical to the explicit conversion.
3. **Given** the global option is registered, **When** an action returns something that is not a
   result, **Then** its response is unchanged.

---

### Edge Cases

- **Extension with the same name as a native default** (e.g. the trace identifier): the Viana value
  wins; customization may still change it.
- **Problem with a non-error status** (e.g. 200): the response keeps the result's status; the problem
  body is still produced.
- **Problem details service not registered by the app**: failures still produce an RFC 9457 body with
  the Viana values and the framework's factory defaults; nothing throws.
- **Title defaulted by the core** (reason phrase, feature 002): it is a Viana value and wins over the
  framework's own default title (names may differ, e.g. 422 "Unprocessable Content").
- **Null payload in a success with a value**: follows the MVC default (the null value is treated as no
  content, 204, unless the app disables `TreatNullValueAsNoContent`). This differs from the old MVC
  package (which writes `null` with the result status) and is recorded as such.
- **Old MVC package installed alongside**: both packages can be referenced without conflicts; the new
  package does not change the old one's behavior.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A new integration package MUST be provided for ASP.NET Core, targeting the supported LTS
  frameworks (net8.0 and net10.0), as the successor of the existing MVC package.
- **FR-002**: Failed results MUST be turned into problem details through the framework's own problem
  details pipeline, so its defaults (type link per status, trace identifier) and the app's global
  customization apply.
- **FR-003**: Precedence MUST be: values set on the Viana problem (status, title, type, detail,
  instance, extension members) > framework defaults; the app customization runs last and can change
  anything.
- **FR-003a** (traceId): The `traceId` member MUST come from the native pipeline; a Viana `traceId`
  extension (any casing) is used only when the native pipeline does not provide one. Same behavior on
  net8.0 and net10.0.
- **FR-004**: For the same values, the produced problem body MUST have the same members and values as
  the framework's own problem response (excluding per-request values) and MUST be RFC 9457 compliant.
- **FR-005**: Failure responses MUST use the problem details media type.
- **FR-006**: Successful results MUST go through the framework's normal response formatting (content
  negotiation, configured formatters/serializers), with the result's status.
- **FR-007**: Body shapes MUST match current library behavior (list results unwrapped, paged results
  wrapped with metadata, no body when the result has none), except a null success value, which follows
  the MVC default for null values (see Edge Cases).
- **FR-008**: An explicit conversion from any result type to an action result MUST be available.
- **FR-009**: An opt-in global registration MUST let actions return results directly (sync and async),
  producing responses identical to the explicit conversion; other action results are untouched.
- **FR-010**: The public surface MUST mirror the existing MVC package where it makes sense (same names
  for the conversion and the global registration), so migration is a package/namespace change.
- **FR-011**: The JSON written by the existing MVC package MUST NOT change in this feature; its only
  change is keeping extension member names as written when the core starts applying naming policies
  (FR-018).
- **FR-012**: The package MUST have XML docs, its own README and an updated example app
  (constitution Principle VIII).
- **FR-013** (OpenAPI, base schema): Every documented problem schema MUST match the native
  `ProblemDetails` schema: members `type`, `title`, `status`, `detail`, `instance`, all optional and
  nullable; extra members allowed in the style of each generator (Swashbuckle `additionalProperties: {}`,
  native OpenAPI 3.1 omits it); `traceId` is NOT declared (native does not declare it either).
- **FR-014** (OpenAPI, source per status): For each status, schema AND example come from one source,
  chosen by precedence: `[ResponseExample]` on the endpoint > `[ProblemResult]` on the action >
  `[ProblemResult]` on the controller > global example (fallback). A global example is ignored when the
  endpoint declares anything for that status. Several endpoint examples for one status → schema is the
  union of their members (members not present in all examples are optional). Applies only to problem
  responses; success examples keep today's behavior.
- **FR-015** (OpenAPI, precedence bug): When the action and its controller declare `[ProblemResult]` for
  the same status, the action MUST win (today the controller overwrites the action).
- **FR-016** (OpenAPI, content type): Documented problem responses MUST use `application/problem+json`
  (matching the runtime response, FR-005, and native `ProducesProblem`), instead of copying the success
  content types (`application/json`, `text/json`, `text/plain`) as today.
- **FR-017** (OpenAPI, shared schema): The base problem schema MUST be emitted once as a shared component
  (`#/components/schemas/ProblemDetails`-style, referenced with `$ref`), like the native generators;
  responses with extra members reference the base and add their members (`allOf` with the base `$ref`).
- **FR-018** (naming policy): Problem extension member names are stored case-insensitively (core rule
  I10) and MUST be written using the application's JSON naming policy: the core JSON converter applies
  the serializer's `PropertyNamingPolicy`; the ASP.NET Core package applies the MVC JSON options policy
  to `ProblemDetails` extensions; OpenAPI schemas and examples apply the same policy. After applying the
  policy, names that collide keep the last value.
- **FR-019** (old MVC package preserved): The old MVC package MUST keep writing extension names exactly
  as stored (original rule), regardless of the naming policy.
- **FR-020** (Newtonsoft): Newtonsoft-specific naming is out of scope (a dedicated package will come
  later); with Newtonsoft, extension names are written as stored.

### Key Entities

- **Result**: Viana outcome (success with optional value, or failure with a problem), including list
  and paged variants.
- **Problem**: Viana RFC 9457 error (status, title, type, extension members incl. detail/instance).
- **Native problem response**: The framework's problem details representation and its defaults.
- **App customization**: The app's global hook to change any problem details before writing.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For every status in a representative set (400, 401, 403, 404, 409, 422, 500), a Viana
  failure and the framework's own problem response with the same values produce bodies with identical
  members and values (excluding per-request values) in 100% of cases.
- **SC-002**: 100% of explicitly set Viana values appear unchanged when no app customization changes
  them.
- **SC-003**: App customization is applied to 100% of Viana failures.
- **SC-004**: Successful responses honor the configured serializer and content negotiation in 100% of
  tested configurations (default JSON, alternative JSON serializer, XML).
- **SC-005**: Migrating the example app from the old MVC package requires changing only the package
  reference and namespaces.
- **SC-006**: Conversion overhead follows Principle V: benchmarked, with no unjustified allocation
  regression versus the old package.

## Clarifications

### Session 2026-11-22 (OpenAPI schema)

- Native behavior verified on net10.0 (Microsoft.AspNetCore.OpenApi 10.0.15, Swashbuckle 10.3.1) with a
  throwaway app: `ProblemDetails` schema = type/title/status/detail/instance, none required, all nullable,
  no `traceId`; action-level declaration overrides controller-level for the same status; controller
  `[ProducesResponseType(typeof(ProblemDetails), 500)]` documents `application/json`/`text/json`/`text/plain`,
  minimal `ProducesProblem(500)` documents `application/problem+json`; schema shared via `$ref`.
- Q: Global example vs endpoint attribute for the same status? → A: Option A — the endpoint declaration
  wins for schema and example; the global example is only a fallback.
- Q: Content type and shared schema (FR-016/FR-017)? → A: Recorded as requirements with the proposed
  direction (`application/problem+json`, shared `$ref` component); confirm before `/speckit-implement`.

### Session 2026-11-22 (traceId)

- Spike (T008): on net10.0 the native pipeline overwrites a preset `traceId`; on net8.0 it keeps it.
- Q: Which `traceId` wins? → A: The MVC/native one, always (both frameworks). The Viana `traceId`
  extension is used only when the native pipeline does not provide one. Documented in the package README.

### Session 2026-11-22 (analysis)

- Q: Null success value? → A: Follow the MVC default (null → 204 unless disabled); a known difference
  from the old package.
- Q: Extension names and naming policy? → A: Case-insensitive storage; written with the app naming
  policy (core converter, ASP.NET Core package, OpenAPI); the old MVC package keeps names as stored;
  Newtonsoft via a future package.

## Assumptions

- net6.0 is not targeted (out of support); apps on net6.0 keep the old MVC package.
- The old package's custom headers/content-type extension points are not carried over; headers are
  set through the normal framework mechanisms.
- Minimal APIs support (feature 008) is out of scope; deprecation of the old package is not planned now; bug fixes from the 2026-11-22 review are feature 007.
- Depends on feature 005 (core: `detail` replaces the obsolete `description`); the mapper promotes only `detail`.
- The package ships as a new package, version 1.0.0.
- ASP.NET Core result-conversion benchmarks (Principle V hot path) are added with this package.
- Work happens on the shared branch `chore/add-spec-kit`.
