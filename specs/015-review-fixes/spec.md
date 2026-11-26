# Feature Specification: Review fixes (envelope cache, empty responses, version residue)

**Feature Branch**: `chore/add-spec-kit`

**Created**: 2026-11-26

**Status**: Ready

**Input**: "Fix what the full code review of the branch found: the envelope shaping decision is cached from one
instance; 401/403 responses are dropped when an operation has no responses; problem examples are dropped when an
operation has no responses. Also remove the mentions of future versions (removal in 2.0.0 / 4.0.0), since nothing
is published."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Envelope shaping does not depend on the first instance (Priority: P1)

A developer's envelope type writes `Status` (or `Problem`) only under a condition, such as "when not default". The
decision to shape the body must come from the type's contract, so `status` and `problem` never reach the body,
whatever the value of the first instance written.

**Why this priority**: it is a correctness bug in the response body, the only finding that affects runtime output.

**Independent Test**: write an envelope whose `Status` is ignored when default, first with status 0 and then with
status 200. Neither body contains `status`.

**Acceptance Scenarios**:

1. **Given** an envelope whose contract can write `Status`/`Problem` under any condition, **When** any instance is
   written, **Then** those members are never in the body.
2. **Given** an envelope whose contract never writes them (`PagedResult`, which uses `[JsonIgnore]`), **When** it
   is written, **Then** the instance itself is the body, with no extra node and the same allocation as today.

---

### User Story 2 - Generated responses are kept on operations without responses (Priority: P2)

When an operation reaches the OpenAPI processing with no responses (null or empty), the 401/403 problem responses
of authorized endpoints and the problem examples that define their own response are still added.

**Why this priority**: today the pipelines always provide at least one response, so it only affects direct use
and edge cases. It is still a silent drop.

**Independent Test**: apply the authorization step to an operation with null responses, and the example step to
an operation with an empty response list and a problem `[ResponseExample]`. Both responses appear.

**Acceptance Scenarios**:

1. **Given** an authorized endpoint whose operation has no responses, **When** the document is processed,
   **Then** 401 and 403 are documented.
2. **Given** a problem `[ResponseExample]` on an operation with no responses, **When** the document is processed,
   **Then** the problem response is documented with its example.
3. **Given** a success example on an operation with no matching response, **When** the document is processed,
   **Then** it is still ignored, as today. Success examples only decorate existing responses.

---

### User Story 3 - No mention of future versions (Priority: P3)

Nothing is published yet, so code comments, XML docs, release notes and test comments must not announce removals
or breaking changes for future versions, such as "will be removed in 2.0.0" or "until it is removed in 4.0.0".
Obsolete APIs stay obsolete, with a message that says what to use instead.

**Why this priority**: documentation consistency, with no behavior change.

**Independent Test**: a search of the repository (src, tests, examples, READMEs, csproj release notes) finds no
version-dated removal or breaking-change announcement.

**Acceptance Scenarios**:

1. **Given** an obsolete member, **When** its message and docs are read, **Then** they name the replacement and no
   version.
2. **Given** the release notes, **When** they are read, **Then** they describe changes without planned versions.

### Edge Cases

- `[Obsolete]` messages are part of the public surface. Changing their text is allowed, because nothing is
  published.
- Historical spec documents under `specs/` are records, not product docs, and are not rewritten.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The envelope shaping decision MUST be derived from the type's serialization contract, not from the
  output of one instance. It may still be cached per contract.
- **FR-002**: Types whose contract never writes `Status`, `Problem` or `Extensions` MUST keep being written as the
  instance itself.
- **FR-003**: The authorization step MUST create the responses collection when it is missing, before adding
  401/403.
- **FR-004**: The example step MUST add problem examples even when the operation has no responses. Success
  examples keep requiring an existing response.
- **FR-005**: No file outside `specs/` MUST announce a future version for removals or breaking changes.
- **FR-006**: Release notes MUST record the fixes; no version bump. READMEs unchanged unless they carry a version
  announcement.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An envelope with conditionally written `Status`/`Problem` never exposes them, in 100% of the test
  cases, regardless of the order of the instances written.
- **SC-002**: The paged and plain success benchmarks keep the allocation recorded in the 014 "after" baseline.
- **SC-003**: The edge-case operations of US2 document every expected response.
- **SC-004**: A repository search outside `specs/` finds zero future-version removal or breaking-change
  announcements.

## Assumptions

- The three code issues come from the full branch review of 2026-11-26. They were verified in the code, and none
  affects the standard pipelines in normal use, except R1 for conditional ignore rules.
- Nothing is published, so changing the text of obsolete messages is not breaking.
