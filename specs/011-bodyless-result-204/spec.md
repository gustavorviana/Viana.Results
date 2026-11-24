# Feature Specification: Bodyless Result is 204, and an opt-out for the inferred success response

**Feature Branch**: `chore/add-spec-kit`

**Created**: 2026-11-24

**Status**: Ready

**Input**: "A Result without a body must automatically be 204. Add a function that removes the automatically
documented result, for minimal APIs and controllers, to be used as a last resort, e.g. when the endpoint
returns the base `Result` but can answer differently, and the developer documents the responses by hand."

## Clarifications

### Session 2026-11-24

- Q: 204 only in the documentation, or also at runtime? → A: Both. `Results.Ok()` without a value answers
  204 and the documentation shows 204, so they always match.
- Q: Version impact? → A: Nothing is published yet (the code is still being built), so there is no major bump.
- Q: Name of the opt-out? → A: `WithoutDefaultResult`: `.WithoutDefaultResult()` on minimal APIs,
  `[WithoutDefaultResult]` on controllers (action or controller).

## Context

Today a minimal API returning a bare `Result` and declaring `Produces(204)` is documented with
"200 (no body)" **and** 204, because ASP.NET Core infers a 200 from the return type and Viana only strips its
body. Controllers do not have the extra 200 when they declare `[ProducesResponseType(204)]`, but without it
they document a bodyless 200. `Results.Ok()` without a value answers 200 with no body, which HTTP semantics
express as 204.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - A bodyless result is a 204, documented and returned (Priority: P1)

A developer returns a `Result` without a value (`Results.Ok()` or `Results.NoContent()`) from a controller
action or a minimal API. The response is 204, and the API document lists 204 (no content), not 200, without
any extra declaration.

**Independent Test**: An endpoint returning `Results.Ok()` answers 204, and its document has a 204 and no 200,
in the native OpenAPI and Swashbuckle documents, for controllers and minimal APIs.

**Acceptance Scenarios**:

1. **Given** an action returning `Results.Ok()`, **When** called, **Then** the status is 204 with no body.
2. **Given** a minimal API returning a bare `Result`, **When** the document is generated, **Then** it lists 204
   and no 200, even if the endpoint also declares `Produces(204)`.
3. **Given** a controller action returning a bare `Result`, **When** the document is generated, **Then** it
   lists 204 without `[ProducesResponseType(204)]`.
4. **Given** a bare `Result` that fails at runtime (for example 404), **Then** the failure keeps its problem
   status; only the success status changes.
5. **Given** `Result<T>`, `ListResult<T>` or `PagedResult<T>`, **Then** nothing changes (200 with the value, or
   the documented status).

---

### User Story 2 - Turning off the inferred success response (Priority: P2)

A developer whose endpoint returns the base `Result` type but can answer with different successes (for
example, one body for one kind of client and another for another kind, or 200 or 202 depending on the
request) marks the endpoint with `WithoutDefaultResult`. The automatically inferred success response
disappears, and only the responses registered by hand (`ProducesResult`, `ProducesProblemResult`,
`[ResponseExample]`, `[ProducesResponseType]`, `[ProblemResult]`) are documented.

**Independent Test**: An endpoint with `WithoutDefaultResult` and two hand-registered successes (200 and 202)
documents exactly those two successes, with no inferred 204 or 200, in both documents and both styles.

**Acceptance Scenarios**:

1. **Given** `.WithoutDefaultResult()` on a minimal API with `ProducesResult<A>()` (200) and
   `ProducesResult<B>(202)`, **Then** the document lists 200 and 202 with their examples and no inferred
   response.
2. **Given** `[WithoutDefaultResult]` on an action, **Then** the same holds for controllers.
3. **Given** `[WithoutDefaultResult]` on a controller class, **Then** it applies to every action, unless an
   action declares otherwise (see FR-008).
4. **Given** `WithoutDefaultResult` and **no** hand-registered success, **Then** the document has no success
   response, and the endpoint's problem responses are unchanged.
5. **Given** `WithoutDefaultResult`, **Then** runtime behavior is unchanged (documentation only).

---

### User Story 3 - Examples show both (Priority: P3)

The catalog examples rely on the new default: the update, delete and inventory toggle routes drop their
explicit `Produces(204)` and `[ProducesResponseType(204)]`. One endpoint per style shows `WithoutDefaultResult`
with a base `Result` that answers different documented successes. The README explains when to use it, as a
last resort.

### Edge Cases

- `Results.Ok()` used today to mean "200 with no body": it becomes 204, which is described in the release notes.
  No published version is affected.
- `Results.Ok(string)` and `Results.Ok(value)` keep answering 200 with the body.
- A bare `Result` built with an explicit 2xx status (`new Result(200)`, `new Result(202)`) keeps that status at
  runtime. The document still infers 204, so declare that status by hand (`[ProducesResponseType(202)]`,
  `Produces(202)`) or use `WithoutDefaultResult` and register the responses.
- An endpoint that declares a different success explicitly (`[ProducesResponseType(202)]`, `Produces(202)`,
  `ProducesResult(...)`) documents that success, and the inferred 204 is not added next to it.
- The Viana.Results.Mvc writer applies the same runtime rule (204 with no body for a bodyless success).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `Results.Ok()` without a value MUST produce a result with status 204.
- **FR-002**: Every writer (AspNetCore filter, minimal API endpoint filter, `ToActionResult`/`ToHttpResult`,
  Mvc `VianaResultAction` and filter) MUST answer a bodyless success with its status and no body. For
  `Results.Ok()`, that status is 204.
- **FR-003**: For an endpoint whose return type is the bare `Result` (directly or through `Task`/`ValueTask`),
  both documents MUST list **204 with no content** as the success, and MUST NOT list a 200, unless a success is
  declared by hand.
- **FR-004**: On minimal APIs, the 200 that ASP.NET Core infers from a Viana result return type is an inferred
  success: FR-003 and FR-005 apply to it exactly as to the MVC default 200.
- **FR-005**: A hand-declared success (any 2xx through `ProducesResult`, `Produces`, `[ProducesResponseType]`
  or `[ResponseExample]`, including the status that an example provider's result carries) MUST be documented
  as declared, and no inferred success MUST be added next to it. This applies to every Viana return type. For
  example, `Result<T>` with a `[ResponseExample]` whose result is `Created` documents only 201.
- **FR-006**: `WithoutDefaultResult()` MUST exist as an extension on minimal API builders (endpoint, group
  and convention builder), and `[WithoutDefaultResult]` as an attribute for actions and controllers.
- **FR-007**: With `WithoutDefaultResult`, the documents MUST contain only the hand-registered responses and
  the endpoint's problem responses. There MUST be no inferred success response (204, 200 or other).
- **FR-008**: The attribute on a controller MUST apply to all its actions. There is no way to turn it back on
  per action (YAGNI); actions that need the default go in a controller without the attribute.
- **FR-009**: `WithoutDefaultResult` MUST NOT change runtime behavior.
- **FR-010**: The behavior MUST be the same in Viana.Results.OpenApi and Viana.Results.OpenApi.Swashbuckle.
- **FR-011**: The XML docs and READMEs MUST describe the 204 default and `WithoutDefaultResult` as a last
  resort, with an example of an endpoint that answers different successes.
- **FR-012**: No package has been published yet, so the core change (FR-001) needs no version bump: the current
  versions stay, and each affected package's release notes describe the new behavior.
- **FR-013**: The examples MUST be updated as in User Story 3, keeping 0 warnings and passing smoke tests.

### Key Entities

- **Default result response**: the success response a document infers from the return type of an endpoint.
- **WithoutDefaultResult marker**: endpoint metadata (minimal APIs) or attribute (controllers) that removes
  the default result response.

## Success Criteria *(mandatory)*

- **SC-001**: 0 endpoints returning a bare `Result` are documented with a 200 in the examples' documents (both
  generators).
- **SC-002**: For every bodyless success built with `Results.Ok()` or `Results.NoContent()`, the runtime status and the documented status match.
- **SC-003**: Endpoints marked `WithoutDefaultResult` document exactly the hand-registered successes (0 extra
  responses), in both styles and both generators.
- **SC-004**: Full solution build with 0 warnings; every existing test passes, apart from the tests that
  asserted the old 200 for `Results.Ok()`, which are updated to 204.

## Assumptions

- "Result without a body" means the non-generic `Result`. `Result<T>` with a null value keeps today's
  behavior (204, as already documented for the native path).
- The opt-out affects only the **inferred** success. Problem responses (from attributes, examples, the global
  examples and the authorization 401/403) are not affected.
- The Mvc package follows the same runtime rule, although it is the legacy path.
- No other mediator library is named in any document.
