# Feature Specification: Clean Up Unused Central Package Entries

**Feature Branch**: `chore/add-spec-kit` (shared branch; no dedicated feature branch)

**Created**: 2026-11-21

**Status**: Draft

**Input**: User description: "Remove unused central package entries from Directory.Packages.props.
Entries that no project references (candidates: Microsoft.AspNet.Mvc, Microsoft.AspNetCore.Mvc.Core,
Newtonsoft.Json, Swashbuckle.AspNetCore.SwaggerGen) must be removed so the central package list
reflects only what the repository actually uses. Each candidate must be verified as unreferenced
before removal; entries still used are kept. Transitive pinning entries added on purpose for security
must not be removed unless no longer needed. After cleanup the repository still restores, builds with
zero warnings and all tests pass on every TFM. No published package changes its dependencies or
version."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Central package list reflects real usage (Priority: P1)

As a maintainer, I want the central package list to contain only packages the repository actually
uses, so that I don't spend time upgrading, auditing or reasoning about dependencies that do nothing.

**Why this priority**: It is the whole feature; unused entries (e.g. an old ASP.NET MVC 5 package and
Newtonsoft.Json) mislead contributors and reviewers about what the libraries depend on.

**Independent Test**: Compare the central list against every project's package references: each
entry is referenced by at least one project, and the build and tests still pass.

**Acceptance Scenarios**:

1. **Given** an entry that no project in `src/`, `tests/` or `examples/` references, **When** the
   cleanup is done, **Then** the entry is gone.
2. **Given** an entry referenced by at least one project, **When** the cleanup is done, **Then** it
   is kept with the same version.
3. **Given** the cleaned list, **When** the repository is restored, built and tested, **Then** it
   succeeds with zero warnings on every target framework.

---

### Edge Cases

- **Entry used only for security pinning of a transitive dependency**: kept, because it is used on
  purpose even without a direct reference. (None exist today; the repository does not enable
  transitive pinning.)
- **Empty sections** left in the central list (e.g. an empty item group): removed.
- **Entry referenced only by an example or test project**: kept; examples and tests are part of the
  repository.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every entry in the central package list MUST be referenced by at least one project in
  the repository, or be a documented, intentional security pin.
- **FR-002**: Each entry MUST be verified as unreferenced before being removed.
- **FR-003**: Entries still in use MUST keep their current version.
- **FR-004**: After cleanup, restore, build and tests MUST succeed with zero warnings (vulnerability
  audit included) on every target framework.
- **FR-005**: No published package MAY change its dependencies, version or release notes.
- **FR-006**: Empty, meaningless sections in the central package list MUST be removed.

### Key Entities

- **Central package entry**: A package name and version declared once for the whole repository.
- **Package reference**: A project's declaration that it uses a package.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of the remaining central entries are used by at least one project.
- **SC-002**: The 4 known unused entries (Microsoft.AspNet.Mvc, Microsoft.AspNetCore.Mvc.Core,
  Newtonsoft.Json, Swashbuckle.AspNetCore.SwaggerGen) are removed, after verification.
- **SC-003**: 0 build warnings and 0 failing tests after the change.
- **SC-004**: 0 published packages change their dependency list or version.

## Assumptions

- Verification at spec time: none of the 4 candidates is referenced by any project; all other 12
  entries are referenced.
- The repository does not enable transitive pinning, so no entry exists only as a security pin.
- This is a repository maintenance change; no NuGet release is needed.
- Work happens on the shared branch `chore/add-spec-kit`.
