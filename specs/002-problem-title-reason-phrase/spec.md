# Feature Specification: Problem Title Defaults to Status Reason Phrase

**Feature Branch**: `chore/add-spec-kit` (shared branch; no dedicated feature branch)

**Created**: 2026-11-21

**Status**: Draft

**Input**: User description: "Default the Problem Details "title" to the standard HTTP reason phrase
of its status (RFC 9457 §4.2.1: when type is "about:blank", title SHOULD be the status code's
reason phrase) instead of the fixed "Error". Today ProblemResult (constructor/WithDescription),
ProblemBuilder and ProblemResultJsonConverter all fall back to "Error" when title is null, empty or
whitespace; this is tested and documented behavior. The new default must be applied consistently by
all three paths (e.g. 404 -> "Not Found", 500 -> "Internal Server Error"). Status codes without a
known reason phrase keep a generic fallback. Explicitly provided titles are never changed. This is
an observable behavior change in the public core package and must be handled per the constitution
(Principle VII: discussed and versioned accordingly); existing tests asserting "Error" must be
updated to the new behavior."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Meaningful default title for problems created in code (Priority: P1)

As a developer using the library, when I create a problem with only a status code (no title), I want
its title to be the standard description of that status (e.g. 404 → "Not Found"), so that API
consumers receive an RFC 9457-compliant, meaningful summary instead of a generic "Error".

**Why this priority**: Creating problems in code (directly or through the builder) is the most
common path; it is what API consumers see in every error response that omits a title.

**Independent Test**: Create a problem with status 404 and no title, both directly and through the
builder; its title is "Not Found". Create one with an explicit title; the title is unchanged.

**Acceptance Scenarios**:

1. **Given** a problem created with status 404 and no title, **When** its title is read, **Then** it
   is "Not Found".
2. **Given** a problem created with status 500 and an empty or whitespace title, **When** its title is
   read, **Then** it is "Internal Server Error".
3. **Given** a problem created through the builder with only status 409, **When** it is built,
   **Then** its title is "Conflict".
4. **Given** a problem created with an explicit title "Order already shipped", **When** its title is
   read, **Then** it is "Order already shipped" regardless of status.

---

### User Story 2 - Same default when reading a problem document (Priority: P2)

As a developer reading Problem Details documents (e.g. from another API), I want a document without a
title to get the same default as a problem created in code, so that behavior is consistent no matter
where the problem came from.

**Why this priority**: Consistency between created and read problems; less frequent than creation.

**Independent Test**: Read a problem document `{"status":404}`; its title is "Not Found". Read one
with a title; the title is preserved.

**Acceptance Scenarios**:

1. **Given** a problem document with status 404 and no title, **When** it is read, **Then** the title
   is "Not Found".
2. **Given** a problem document with no status and no title, **When** it is read, **Then** the status
   defaults to 500 (current behavior) and the title is "Internal Server Error".
3. **Given** a problem document with a title, **When** it is read, **Then** the title is preserved.

---

### Edge Cases

- **Status without a known reason phrase** (e.g. 499, 299, 0, negative): the title falls back to the
  generic "Error" (current default), never to an empty value.
- **Placeholder entries in the status table** (306 "Unused", 418 "(Unused)"): treated as having no
  usable reason phrase, so the generic fallback applies.
- **Title with only whitespace**: treated as missing (current behavior), so the default applies.
- **Custom problem type** (`type` other than `about:blank`) with no title: the reason-phrase default
  applies as well (same rule for every type). Consumers who need a type-specific title provide it
  explicitly or customize it in the host framework (constitution Principle IX).
- **Round trip**: a problem whose title was defaulted keeps that title after being written and read
  back.

## Clarifications

### Session 2026-11-21

- Q: Apply the reason-phrase default to custom types too? → A: Yes, to every problem without a title; not configurable in the core.
- Q: Version classification? → A: MINOR (`Viana.Results` 2.1.0, from the current 2.0.0), with release notes calling out the change.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: When a problem has no title (null, empty or whitespace), its title MUST default to the
  standard reason phrase of its status code (e.g. 400 "Bad Request", 404 "Not Found",
  500 "Internal Server Error").
- **FR-002**: The default MUST be identical whether the problem is created directly, created through
  the builder, or read from a Problem Details document.
- **FR-003**: When the status code has no known reason phrase, or the known entry is a placeholder,
  the title MUST fall back to "Error".
- **FR-004**: An explicitly provided, non-blank title MUST never be changed.
- **FR-005**: Existing tests and documentation that assert or describe the "Error" default MUST be
  updated to the new behavior; the generic fallback (FR-003) MUST be covered by tests.
- **FR-006**: The change MUST be released with a version bump and release notes that describe the
  behavior change, following the constitution's versioning rules. It is classified as a compatible
  improvement (MINOR, `Viana.Results` 2.1.0, from the current 2.0.0): RFC 9457 treats `title` as advisory, so clients must
  not rely on its exact value. The release notes MUST call out the new default.
  Update 2026-11-21 (owner decision): shipped in `Viana.Results` 3.0.0 together with feature 005.
- **FR-007**: The default `type` behavior ("about:blank" when missing) MUST remain unchanged, and an
  explicitly provided `type` MUST never be changed.
- **FR-008**: Default titles are not configurable in the core package; customization happens in the
  host framework (constitution Principle IX).

### Key Entities

- **Problem**: An RFC 9457 error description with status, type, title and extension members.
- **Reason phrase**: The standard short description of an HTTP status code (e.g. 404 → "Not Found").

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of problems created or read without a title, for any status with a known reason
  phrase, expose that reason phrase as their title.
- **SC-002**: 0 problems end up with an empty title, for any status code.
- **SC-003**: 100% of explicitly provided titles are preserved unchanged.
- **SC-004**: The same input (status, no title) yields the same title through all three entry points
  (direct creation, builder, reading a document) in 100% of tested cases.

## Assumptions

- The reason phrases used are the standard HTTP ones already maintained by the library (IANA
  registry names, e.g. 413 "Content Too Large", 422 "Unprocessable Content").
- The status default when reading a document without status (500) is unchanged.
- The friendlier, sentence-style messages the library already uses elsewhere (e.g. "The Requested
  Resource Was Not Found.") are not used as titles; titles stay short reason phrases.
- Only the core package changes behavior; integration packages inherit the new default
  automatically. Their tests are updated only if they assert the old "Error" title.
- Work happens on the shared branch `chore/add-spec-kit` (no dedicated feature branch).
