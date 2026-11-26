# Feature Specification: Validate response declarations against the return type

**Feature Branch**: `chore/add-spec-kit`

**Created**: 2026-11-26

**Status**: Ready

**Input**: "Viana response declarations on endpoints that do not return a Viana result are handled inconsistently:
on controllers `[ProblemResult]` and the default result are silently ignored, `[ResponseExample]` problem
examples are still applied and success examples are dropped silently; on minimal APIs the declarations are
applied whatever the delegate returns. Make it consistent: declaring on something that does not return a result
is an error; on a controller class, only the result actions receive the declarations and the others are
skipped with a warning; a controller class with no result action is an error."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Declaration on a non-result minimal API fails (Priority: P1)

A developer adds `ProducesResult<User>()`, `ProducesProblemResult(...)`, `WithoutDefaultResult()` or an example
provider to a minimal API whose handler does not return a Viana result. Today the document describes responses
the endpoint never produces. After this feature, generating the document fails with a message naming the
endpoint and the declaration.

**Why this priority**: the document is wrong and nothing reports it. Failing early is the cheapest way to catch it.

**Independent Test**: map a handler returning `string` with `ProducesResult<User>()`, generate the document and
check that it fails naming the endpoint; the same declaration on a handler returning `Result<User>` succeeds.

**Acceptance Scenarios**:

1. **Given** a minimal API returning a non-result type with any Viana declaration, **When** the document is
   generated, **Then** generation fails and the message names the endpoint (method and route) and the
   declaration.
2. **Given** a minimal API returning `Result`, `Result<T>` or another Viana result (directly or wrapped in a
   task), **When** the document is generated, **Then** the declarations apply exactly as today.

---

### User Story 2 - Declaration on a non-result controller action fails (Priority: P1)

A developer puts `[ProblemResult]`, `[ResponseExample]` or `[WithoutDefaultResult]` directly on an action that
does not return a Viana result. Today some are ignored and some applied. After this feature, it is an error
naming the controller, the action and the attribute.

**Why this priority**: it is the same inconsistency as US1, on the controller side. Both should behave alike.

**Independent Test**: an action returning a DTO with `[ProblemResult(404)]` makes generation fail; the same
attribute on an action returning `Result<Dto>` works.

**Acceptance Scenarios**:

1. **Given** an action not returning a Viana result with a Viana attribute on the action itself, **When** the
   document is generated, **Then** generation fails naming controller, action and attribute.
2. **Given** an action returning a Viana result with the same attributes, **When** the document is generated,
   **Then** the document is unchanged from today.

---

### User Story 3 - Declaration on a controller class applies only to result actions (Priority: P2)

A developer puts a Viana attribute on the controller class. Actions returning a Viana result receive it. Actions
that do not return one are skipped, and a warning is logged naming the controller, the action and the attribute.
If no action of the controller returns a Viana result, generation fails.

**Why this priority**: class-level attributes are convenient on mixed controllers, and skipping with a warning
keeps them usable. A class with no result action is clearly a mistake, so it fails.

**Independent Test**: a controller with `[ProblemResult(500)]` on the class and two actions (one returning
`Result<T>`, one returning a DTO): the first documents 500, the second does not, and one warning is logged for
the second. A controller whose actions all return DTOs fails generation.

**Acceptance Scenarios**:

1. **Given** a class-level Viana attribute and a mix of result and non-result actions, **When** the document is
   generated, **Then** only result actions get the declaration and each skipped action logs one warning.
2. **Given** a class-level Viana attribute and no action returning a Viana result, **When** the document is
   generated, **Then** generation fails naming the controller and the attribute.
3. **Given** a class-level attribute inherited from a base controller, **When** the document is generated,
   **Then** the same rules apply to the derived controller's actions.

### Edge Cases

- Every Viana result type counts as a result: `IResult`, `IResult<T>`, `IListResult<T>`, `IPagedResult<T>`,
  `Result`, `Result<T>`, `ListResult<T>`, `PagedResult<T>` and any custom type that implements the Viana
  `IResult`.
- Only the action-return wrappers are unwrapped: `Task<T>`, `ValueTask<T>` (as today) and `ActionResult<T>`,
  nested in any order, for example `Task<ActionResult<Result<User>>>` or `Task<IResult>` (Viana). Any other
  generic counts as non-result, including collections (`List<Result>`, `IEnumerable<Result<T>>`) and
  `Dictionary<string, Result>`.
- An action declared as the Viana `IResult` (as used with the Mvc filter) counts as a result.
- Returns that carry a result inside a generic wrapper, such as `ActionResult<Result<T>>`, were ignored before
  this feature and now receive the declarations (204, `[ProblemResult]`, default result). This is a visible
  change, accepted because nothing is published.
- With several documents (for example v1 and v2), the warning is logged once per generated document.
- Non-generic types that may hide a Viana result at runtime (`IActionResult`, `ActionResult`, `object`, the
  ASP.NET Core `IResult`) count as non-result: the rules of FR-001 to FR-005 apply to them.
- `void`/`Task` actions are non-result.
- Global examples registered in the options are not endpoint declarations. They apply only to result endpoints
  and never produce errors or warnings.
- The warning is emitted once per action and attribute per document generation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A minimal API with any Viana response declaration whose handler does not return a Viana result
  MUST fail document generation with a message naming the endpoint and the declaration.
- **FR-002**: A controller action carrying a Viana response attribute directly MUST fail document generation
  when it does not return a Viana result; the message names controller, action and attribute.
- **FR-003**: A Viana response attribute on a controller class (own or inherited) MUST apply only to actions
  returning a Viana result.
- **FR-004**: Each action skipped under FR-003 MUST log one warning naming controller, action and attribute.
- **FR-005**: A Viana response attribute on a controller class with no action returning a Viana result MUST fail
  document generation naming the controller and the attribute.
- **FR-006**: Declarations on endpoints whose return type was already recognized as a Viana result (directly or
  through `Task`/`ValueTask`) MUST produce the same document as today. Generic wrappers newly recognized under
  the edge-case rule (such as `ActionResult<Result<T>>`) now receive the declarations.
- **FR-007**: The rules MUST behave the same in both supported OpenAPI pipelines (native and Swashbuckle).
- **FR-008**: The release notes MUST describe the rules; no version bump. READMEs stay minimal (at most one line); the full rules go to the wiki.

### Key Entities

- **Viana response declaration**: `[ProblemResult]`, `[ResponseExample]`, `[WithoutDefaultResult]` on
  controllers; `ProducesResult`, `ProducesProblemResult`, `WithoutDefaultResult` and example providers on minimal
  APIs.
- **Result endpoint**: an endpoint whose declared return type is a Viana result, possibly wrapped (recursively)
  in `Task<T>`, `ValueTask<T>` or `ActionResult<T>`.

## Clarifications

### Session 2026-11-26

- Q: How are declared types that may hide a Viana result at runtime treated? → A: A generic type whose generic
  argument is a Viana result is accepted; anything else (including `IActionResult`, `object`) is refused as
  non-result.
- Q: Which generics count? → A: Only the wrappers used to declare an action's return (`Task<T>`, `ValueTask<T>`,
  `ActionResult<T>`), nested. Collections and every other generic are refused.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every declaration on a non-result endpoint is either reported (error or warning) or applied by a
  documented rule. None is silently ignored.
- **SC-002**: Every error and warning names the endpoint and the declaration, so the developer finds the cause
  without debugging.
- **SC-003**: Existing tests for result endpoints keep passing without changes to their expected documents.
- **SC-004**: Controller and minimal API cases with the same declaration give the same outcome (tested in
  pairs).

## Assumptions

- Nothing is published yet, so turning silent cases into errors needs only release notes.
- Warnings go through the application's standard logging, under the OpenApi package's category.
- Validation happens when the document is generated, not at application startup.
