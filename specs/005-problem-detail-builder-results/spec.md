# Feature Specification: RFC 9457 "detail" and Builder Results

**Feature Branch**: `chore/add-spec-kit` (shared branch; no dedicated feature branch)

**Created**: 2026-11-21

**Status**: Draft

**Input**: User request: "Mark the public `description` APIs (e.g. `public static ProblemResult
WithDescription`) as obsolete, since `description` is not the correct RFC 9457 member; point users to
`detail`. The obsolete message must say the API will be removed in the next major version (one after
the major created for the current work). `ProblemBuilder` must offer a way to return a result directly
with the problem inside, since failures are normally used as a result holding the problem."

## Clarifications

### Session 2026-11-21

- Q: Where does this change live? → A: Its own core feature (005), implemented before the ASP.NET Core
  package (006), so the integration maps only `detail`.
- Q: Version? → A: The current work ships as a new MAJOR of the core: `Viana.Results` 3.0.0 (it
  includes features 002 and 005, unreleased). The obsolete APIs are removed in the next major, 4.0.0.
- Q: Builder method names? → A: `BuildResult()` and `BuildResult<T>()` (no list/paged variants).
- Q: OpenAPI `ProblemResultAttribute.Description` (documented but ignored)? → A: Option A, as a user
  story of this feature: add `Detail` wired into the documented schema and deprecate `Description`
  (removal in the next major of the OpenAPI package, 2.0.0).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Use the standard "detail" member (Priority: P1)

As a developer creating problems, I want a direct way to set the RFC 9457 `detail` member and to be
warned when I use the non-standard `description`, so that my error responses follow the standard and
API clients find the explanation where they expect it.

**Why this priority**: `description` is not an RFC 9457 member; clients and tools look for `detail`.

**Independent Test**: Create a problem with the new detail factory; it carries `detail`. Calling a
`description` API produces a compiler warning that names `detail` as the replacement and 4.0.0 as the
removal version, while still behaving as before.

**Acceptance Scenarios**:

1. **Given** a status, title and explanation text, **When** a problem is created with the detail
   factory, **Then** it carries the text as `detail` and no `description`.
2. **Given** existing code calling `ProblemResult.WithDescription` or `ProblemBuilder.WithDescription`,
   **When** it is compiled, **Then** it gets an obsolete warning saying `description` is not an RFC 9457
   member, to use the `detail` API instead, and that it will be removed in 4.0.0.
3. **Given** existing code calling a `description` API, **When** it runs, **Then** its behavior is
   unchanged (the text is still stored as `description`).
4. **Given** a Problem Details document containing `description`, **When** it is read, **Then** it is
   still accepted and kept (no behavior change for readers).

---

### User Story 2 - Build a result with the problem inside (Priority: P1)

As a developer, I want the problem builder to return the failure result directly, so that I don't
need an extra step to wrap the problem in a result — failures are normally returned as a result
holding the problem.

**Why this priority**: Most call sites return a result, not a bare problem.

**Independent Test**: `new ProblemBuilder(404).WithDetail("x").BuildResult()` returns a failed result
whose status is 404 and whose problem has `detail` "x"; the typed variant returns a typed failed
result.

**Acceptance Scenarios**:

1. **Given** a configured builder, **When** the untyped result is built, **Then** it is a failure with
   the builder's status and a problem equal to what the builder's problem build produces.
2. **Given** a configured builder, **When** a typed result is built for a value type, **Then** it is a
   failure of that type, with no value, the same status and the same problem.
3. **Given** the same builder, **When** both the problem and the result are built, **Then** the
   problem inside the result has the same members and values as the problem built directly.

---

### User Story 3 - Document "detail" in OpenAPI problem responses (Priority: P2)

As a developer documenting my API, I want to declare the `detail` text of an error response on the
endpoint and see it in the generated OpenAPI document, and to be warned that the current
`Description` option has no effect and is being replaced.

**Why this priority**: Today the documented `Description` silently does nothing; documentation should
match the standard member that the runtime now favors.

**Independent Test**: An endpoint annotated with a problem response for 400 with `Detail = "One or more
fields are invalid."` produces, in the OpenAPI document, a 400 problem schema with a `detail` member
whose example is that text. Using `Description` compiles with an obsolete warning naming `Detail` and
the removal version.

**Acceptance Scenarios**:

1. **Given** a documented problem response with a detail text, **When** the OpenAPI document is
   generated (native pipeline and Swashbuckle), **Then** the problem schema for that status includes a
   `detail` string member with the text as example.
2. **Given** a documented problem response without detail, **When** the document is generated,
   **Then** the schema is unchanged from today (no `detail` member).
3. **Given** code using the `Description` option, **When** it is compiled, **Then** it gets an obsolete
   warning saying it has no effect, to use `Detail`, and that it will be removed in
   `Viana.Results.OpenApi` 2.0.0; the generated document is unchanged.

---

### Edge Cases

- **Both `description` and `detail` set on the builder**: both are kept as today (no merge).
- **Empty or null detail**: not stored (same rule as other optional members).
- **Obsolete usage inside the library itself**: the library must not call its own obsolete APIs (the
  build treats warnings as errors).
- **Tests of the obsolete APIs**: still exercised until removal, with a narrow, commented suppression.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A factory to create a problem with the RFC 9457 `detail` member MUST be added, mirroring
  the existing `description` factory (status, title, detail, type, extensions).
- **FR-002**: Every public API whose purpose is setting `description` MUST be marked obsolete with a
  message stating: `description` is not an RFC 9457 member; use the `detail` equivalent; it will be
  removed in `Viana.Results` 4.0.0.
- **FR-003**: Obsolete APIs MUST keep their current behavior until removal.
- **FR-004**: Reading Problem Details documents containing `description` MUST keep working.
- **FR-005**: The problem builder MUST offer `BuildResult()` returning a failed untyped result and
  `BuildResult<T>()` returning a failed typed result, each holding the built problem and its status.
- **FR-006**: The library MUST NOT call its own obsolete APIs internally.
- **FR-007**: Documentation (XML docs, core README, examples) MUST use `detail` and the new builder
  methods instead of `description`.
- **FR-008**: The core package version MUST be 3.0.0, with release notes listing features 002 and 005
  and the deprecations with their removal version (4.0.0).
- **FR-009**: Other packages in the repository MUST NOT call the obsolete APIs either (examples
  migrated to `detail`).
- **FR-010**: The OpenAPI problem-response annotation MUST offer a `Detail` option; when set, the
  documented problem schema MUST include a `detail` string member with that text as example, for both
  OpenAPI pipelines (native and Swashbuckle).
- **FR-011**: The annotation's `Description` option MUST be marked obsolete with a message stating it
  has no effect, to use `Detail`, and that it will be removed in `Viana.Results.OpenApi` 2.0.0.
- **FR-012**: The OpenAPI package version MUST be bumped as MINOR (1.1.0) with release notes; the
  Swashbuckle package as PATCH (1.1.2), since it only reuses the annotation.

### Key Entities

- **Problem**: RFC 9457 error; `detail` is the standard explanation member; `description` is a legacy,
  non-standard extension.
- **Result**: failure outcome holding a problem and its status.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of public `description` setters/factories are marked obsolete with the 4.0.0
  removal notice and a `detail` replacement.
- **SC-002**: 0 internal calls to obsolete APIs in library code and examples (build stays at 0 warnings).
- **SC-003**: 100% of existing `description` behaviors still pass their tests.
- **SC-004**: A failure result can be produced from the builder in a single call, for untyped and
  typed results.
- **SC-005**: 100% of documented problem responses with a detail text show it in the generated
  OpenAPI document (both pipelines); documents without detail are unchanged.

## Assumptions

- Public `Description` properties in the OpenAPI packages that document OpenAPI examples/fields
  (`ResponseExampleAttribute.Description`, schema field descriptions) are a different concept (OpenAPI
  `description`) and are not deprecated.
- Version 2.1.0 was never published; its content ships in 3.0.0.
- Work happens on the shared branch `chore/add-spec-kit`.
