# Feature Specification: Envelope results

**Feature Branch**: `chore/add-spec-kit`

**Created**: 2026-11-26

**Status**: Draft

**Input**: "Add `IEnvelopeResult<T> : IResult<T>` (no non-generic variant). The body of an envelope result is the
whole object: every public property except `Status` and `Problem`. When the type implements `IHasExtensions`,
the `Extensions` property is not written and its entries are copied to the root of the body; a declared property
wins over an extension with the same name. Problems and 204 behave as today. `PagedResult` implements the
contract and the hard-coded `IPagedResult<>` exception is removed, so one rule serves every package. The OpenAPI
document describes the same shape. `[JsonPropertyName]` is honored (test only). Use case: a `CursorResult<T>`
with `Data` and `NextCursor`."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Custom envelope keeps its metadata in the response (Priority: P1)

A developer creates a `CursorResult<T>` with `Data` and `NextCursor` that implements `IEnvelopeResult<T>`, and
returns it from an endpoint. Today only `Data` reaches the client and the cursor is lost. After this feature, the
body is `{ "data": [...], "nextCursor": "abc" }` on minimal APIs, controllers (Mvc and AspNetCore interop) alike.

**Why this priority**: this is the problem the feature exists for. Without it, list results with metadata cannot
be built by users.

**Independent Test**: return a `CursorResult<User>` from a minimal API and from a controller and compare the
body with the expected JSON.

**Acceptance Scenarios**:

1. **Given** an envelope result with data and extra public properties, **When** it is returned, **Then** the
   body contains every public property and never `status` or `problem`.
2. **Given** an envelope result whose property has `[JsonPropertyName("items")]`, **When** it is returned,
   **Then** the body uses `items`.
3. **Given** an envelope result carrying a problem, **When** it is returned, **Then** the response is the
   problem (`application/problem+json`), as for any result.
4. **Given** an envelope result with status 204, **When** it is returned, **Then** there is no body.
5. **Given** a null `Data`, **When** it is returned, **Then** the body still has the other properties and `data`
   is written as null.

---

### User Story 2 - Extensions go to the root of the envelope (Priority: P2)

A developer's envelope also implements `IHasExtensions`, for example to add dynamic metadata. The extension
entries appear at the root of the body next to the declared properties. The `extensions` property itself is not
written.

**Why this priority**: it is a refinement of US1 for dynamic metadata.

**Independent Test**: an envelope with `Extensions = { "region": "eu", "data": "x" }` produces
`{ "data": [...], "nextCursor": ..., "region": "eu" }`. The declared `data` wins over the extension.

**Acceptance Scenarios**:

1. **Given** an envelope with extensions, **When** it is returned, **Then** each entry is a root member and there
   is no `extensions` member.
2. **Given** an extension whose name equals a declared property's JSON name, **When** it is returned, **Then**
   the declared property's value is written and the extension is dropped.

---

### User Story 3 - PagedResult follows the same rule (Priority: P2)

`PagedResult<T>` becomes an envelope result. Its body stays `{ "data": [...], "pageNumber": n, "totalPages": m }`,
and the special case for paged results disappears, so the core, Mvc, AspNetCore and OpenApi follow one rule.

**Why this priority**: it removes the special case and proves the rule on an existing type. The output must not
change.

**Independent Test**: existing paged-result tests on every package pass unchanged.

**Acceptance Scenarios**:

1. **Given** a `PagedResult<T>`, **When** it is returned or documented, **Then** the body and schema are
   identical to today.

---

### User Story 4 - The OpenAPI document matches the runtime (Priority: P1)

For an endpoint returning an envelope result, the OpenAPI document (native and Swashbuckle) describes the whole
envelope: every public property, with `[JsonPropertyName]` names. When the type implements `IHasExtensions`, the
schema allows additional properties. The document never shows `status`, `problem` or `extensions`.

**Why this priority**: a body that differs from its documentation is the kind of bug the previous features fixed.

**Independent Test**: generate the document for a `CursorResult<User>` endpoint on both pipelines and compare
the schema's properties with the runtime body's members.

**Acceptance Scenarios**:

1. **Given** an envelope endpoint, **When** the document is generated, **Then** the success schema has exactly
   the envelope's public properties (JSON names), on both pipelines.
2. **Given** an envelope that implements `IHasExtensions`, **When** the document is generated, **Then** the
   schema allows additional properties and has no `extensions` property.
3. **Given** a response example built from an envelope instance, **When** the document is generated, **Then**
   the example is the same JSON the runtime writes.

### Edge Cases

- Non-envelope results keep today's rules: `Result<T>` is unwrapped to `Data`, scalar data stays wrapped, and a
  non-generic `Result` has no body.
- An envelope that derives from `ListResult<T>` or `Result<T>` follows the envelope rule, not the unwrap rule.
- Public properties marked `[JsonIgnore]` are not written and not documented.
- The application's JSON options (naming policy, converters) apply to the envelope like to any body.
- Extension values are serialized with their runtime type.
- Mediator dispatch is not affected. The envelope is only a response shape.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The core MUST expose `IEnvelopeResult<T>`, derived from `IResult<T>`, with XML documentation. There
  is no non-generic variant.
- **FR-002**: The response body of an envelope result MUST be the object's public properties, excluding
  `Status`, `Problem` and (when present) `Extensions`, serialized with the application's JSON options.
- **FR-003**: For an envelope that implements `IHasExtensions`, each extension entry MUST be written at the root
  of the body. A declared property MUST win over an extension with the same JSON name.
- **FR-004**: Problems and 204 MUST behave as for every other result.
- **FR-005**: `PagedResult<T>` MUST implement `IEnvelopeResult<IReadOnlyList<T>>`, and the paged-result special
  case MUST be removed. Its body and schema MUST stay identical.
- **FR-006**: The Mvc, AspNetCore and OpenApi packages (native and Swashbuckle) MUST all apply the same envelope
  rule.
- **FR-007**: The OpenAPI schema of an envelope MUST list the same members the runtime writes, allow additional
  properties when the type has extensions, and never show `status`, `problem` or `extensions`.
- **FR-008**: `[JsonPropertyName]` on envelope properties MUST be honored by the runtime and the document. No
  new option is added for naming.
- **FR-009**: The release notes MUST describe envelope results; no version bump. The core README gets at most a
  short `CursorResult<T>` snippet; the full rules go to the wiki.
- **FR-010**: An example using a cursor envelope MUST be added to the examples (one type per file).

### Key Entities

- **Envelope result**: a result whose whole object (minus status, problem and extensions container) is the
  response body.
- **Extension entries**: dynamic members copied to the body's root, lower priority than declared properties.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user-defined cursor result needs no code beyond the type itself to return its metadata. There is
  no registration and no attribute.
- **SC-002**: For every envelope test case, the runtime body and the documented schema have the same members,
  in all packages.
- **SC-003**: All existing paged-result tests pass without changes to their expected output.
- **SC-004**: The core keeps its trimming and Native AOT compatibility (the AOT smoke test passes).

## Assumptions

- The envelope's JSON is produced by the application's serializer. The core only decides which object (or which
  members) becomes the body.
- The AOT constraints of the core are handled in the plan. Satellites already serialize with the host options.
- Nothing is published, so no migration guide is needed.
- Spec 013 is independent. Envelope results are Viana results for its rules.
