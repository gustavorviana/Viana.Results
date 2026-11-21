# Feature Specification: Warnings as Errors

**Feature Branch**: `chore/add-spec-kit` (shared branch; no dedicated feature branch)

**Created**: 2026-11-21

**Status**: Draft

**Input**: User description: "Enable TreatWarningsAsErrors across all packages in src/ so that any
compiler, nullable, XML documentation or analyzer warning fails the build, as required by the
constitution's Technical Constraints. Existing warnings must be fixed (not suppressed) unless a
suppression is individually justified. CI must fail on any new warning. Test and example projects
should follow the same rule unless there is a documented reason not to."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Published packages build with zero warnings (Priority: P1)

As the library maintainer, I want every package that is published to NuGet to fail its build
whenever any warning is produced, so that quality problems (nullability mistakes, missing public
API documentation, analyzer findings, vulnerable dependencies) can never reach consumers silently.

**Why this priority**: The published packages are what consumers depend on. This is the core of
the constitution's "no new warnings" rule and the prerequisite for the upcoming AOT work, whose
compatibility checks are reported as warnings.

**Independent Test**: Introduce a deliberate warning (e.g., a public member without
documentation) in any published package and build it; the build must fail. Remove it; the build
must succeed with zero warnings.

**Acceptance Scenarios**:

1. **Given** the current code base, **When** all published packages are built for every target
   framework, **Then** the build succeeds and reports zero warnings.
2. **Given** a published package, **When** a contributor introduces any compiler, nullable,
   documentation or analyzer warning, **Then** the build of that package fails and names the
   offending warning.
3. **Given** a warning that exists today, **When** this feature is complete, **Then** that warning
   has been fixed at its source rather than hidden, unless a justified exception is recorded.

---

### User Story 2 - CI blocks any new warning (Priority: P2)

As the maintainer reviewing a Pull Request, I want CI to fail when a change introduces a warning,
so that the zero-warning state is enforced automatically instead of relying on reviewers.

**Why this priority**: Enforcement locally is not enough; `main` is protected and only CI is a
reliable gate for every contributor.

**Independent Test**: Open a Pull Request containing a deliberate warning; CI must report a
failure. Remove the warning; CI must pass.

**Acceptance Scenarios**:

1. **Given** a Pull Request that introduces a warning in any project of the solution, **When** CI
   runs, **Then** the CI run fails.
2. **Given** a Pull Request with no warnings, **When** CI runs, **Then** the CI run passes.

---

### User Story 3 - Tests and examples follow the same rule (Priority: P3)

As a contributor, I want test and example projects to follow the same zero-warning rule, so that
the whole repository has one consistent standard and examples shown to users are clean.

**Why this priority**: Valuable for consistency, but these projects are not shipped, so the impact
of a warning there is lower than in published packages.

**Independent Test**: Introduce a deliberate warning in a test project and in an example project;
both builds must fail, except for warning categories covered by a documented exception.

**Acceptance Scenarios**:

1. **Given** a test or example project, **When** a warning is introduced, **Then** the build fails.
2. **Given** an example project that is exempt from a specific warning category, **When** that
   category occurs, **Then** the build succeeds, and the exemption and its reason are written down
   next to the exemption itself.

---

### Edge Cases

- **Warnings that appear only for some target frameworks**: every target framework of every
  project must build warning-free, not only the newest one.
- **Vulnerable dependencies** (today: a high-severity advisory on a transitive dependency of the
  OpenAPI packages): the warning must be resolved by moving to a non-vulnerable version, not by
  hiding it.
- **New security advisories published after merge**: a dependency that was clean can become
  "vulnerable" without any code change. Such advisories MUST break the build like any other
  warning, both locally and in CI (see FR-009); the expected remedy is upgrading the dependency.
- **Warnings in generated code or from third-party analyzers**: treated the same as any other
  warning unless a justified exception is recorded.
- **A suppression is genuinely needed**: allowed only at the narrowest possible scope, with a
  written reason next to it.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every published package MUST fail to build when any warning is produced, for every
  target framework it supports.
- **FR-002**: Every test project and every example project MUST fail to build when any warning is
  produced, except for warning categories covered by a documented exception (FR-005).
- **FR-003**: The rule MUST be defined once for the whole repository, so that new projects inherit
  it automatically without per-project configuration.
- **FR-004**: All warnings present when this feature starts MUST be fixed at their source,
  including the vulnerable transitive dependency currently reported for the OpenAPI packages.
- **FR-005**: Any suppression or exemption MUST be as narrow as possible (single warning code,
  single project or single code location) and MUST carry a written justification next to it.
  The existing exemption of missing XML documentation in the shared examples project is kept only
  if justified this way.
- **FR-006**: CI MUST fail when any project in the solution produces a warning.
- **FR-007**: Local builds and CI builds MUST apply the same rule, so that a build that passes
  locally does not fail in CI for warning reasons (and vice versa).
- **FR-008**: The project's contributor documentation MUST state the zero-warning rule and how to
  request a justified exception.
- **FR-009**: Dependency vulnerability warnings MUST fail the build like any other warning, both
  locally and in CI, including advisories published after a change was merged.
- **FR-010**: Null-forgiving operators used only to silence the compiler in published code MUST be
  replaced by null-safe code. Observable behavior MUST NOT change: a Problem Details document read
  without `type` or `title` keeps the current defaults (`type` = `about:blank`, `title` = `Error`).
  Aligning the default `title` with the status reason phrase (RFC 9457 §3.1.4) is out of scope and
  tracked as a separate feature.

## Clarifications

### Session 2026-11-21

- Q: Should newly published vulnerability advisories break the build like any other warning, or
  be reported without failing it? → A: They always break the build, locally and in CI (security
  over build predictability for a public library).
- Q (during implementation): Should the default `title` change from `Error` to the status reason
  phrase? → A: No, not in this feature. Only remove the null-forgiving operators without behavior
  change; the reason-phrase default becomes a separate feature (it is tested, documented behavior
  shared by `ProblemResult`, `ProblemBuilder` and the converter).

### Key Entities

- **Warning**: A diagnostic emitted during build (compiler, nullability, documentation, analyzer,
  or dependency audit), identified by a code.
- **Justified exception**: A narrowly scoped suppression of a specific warning code, accompanied by
  a written reason.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A full build of the solution, across all target frameworks, reports 0 warnings.
- **SC-002**: 100% of projects in the repository (packages, tests, examples) are covered by the
  rule, including any project added later, with no per-project setup.
- **SC-003**: A deliberately introduced warning causes CI to fail in 100% of cases.
- **SC-004**: 0 known vulnerable dependencies remain in the dependency graph.
- **SC-005**: Every suppression in the repository has a written justification (100% coverage on
  review).

## Assumptions

- The rule applies to all warning sources produced by the standard build: compiler, nullability,
  XML documentation, analyzers and dependency vulnerability audit.
- Fixing the vulnerable dependency does not require a breaking change to the public API of the
  OpenAPI packages; if it does, it will be discussed per the constitution before proceeding.
- Example projects are not published, so exempting them from the XML documentation requirement is
  an acceptable, justified exception; all other warnings still apply to them.
- No new third-party analyzer packages are introduced by this feature; only warnings already
  produced by the current build configuration are in scope.
- Work happens on the shared branch `chore/add-spec-kit` (no dedicated feature branch), per the
  maintainer's decision.
