# Feature Specification: Core Benchmarks

**Feature Branch**: `chore/add-spec-kit` (shared branch; no dedicated feature branch)

**Created**: 2026-11-21

**Status**: Draft

**Input**: User description: "Add a benchmarks/ project using BenchmarkDotNet to measure the hot
paths of the core package (Viana.Results), per constitution Principle V. Cover: creating
Result/Result<T> (success and failure), implicit conversions, ProblemResult/ProblemBuilder creation
(with and without title, to cover the reason-phrase default), and ProblemResultJsonConverter
serialization/deserialization (with and without extensions). Report time and allocations
(MemoryDiagnoser). The project is not packed or published, is excluded from the test run, follows
TreatWarningsAsErrors, and runs only on demand (not in CI by default). A README in benchmarks/
explains how to run them and how to compare results before/after a change. Baseline results for the
current version are recorded in the repository."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Measure the core hot paths on demand (Priority: P1)

As a maintainer, I want to run a single command that measures the time and memory allocated by the
most common operations of the core package, so that I can back performance claims with numbers
(constitution Principle V).

**Why this priority**: Without measurements, no performance-related change can be evaluated.

**Independent Test**: Run the documented command; a report lists every covered scenario with mean
time and allocated memory per operation.

**Acceptance Scenarios**:

1. **Given** a clean checkout, **When** the maintainer runs the documented command, **Then** a report
   is produced with time and allocated memory for each covered scenario.
2. **Given** the maintainer wants only one group (e.g. JSON), **When** they pass a filter as
   documented, **Then** only that group runs.
3. **Given** the normal build and test commands, **When** they run (locally or in CI), **Then** the
   benchmarks are built but not executed, and no benchmark artifact is packed or published.

---

### User Story 2 - Compare before and after a change (Priority: P2)

As a maintainer or contributor, I want a recorded baseline and documented steps to compare a change
against it, so that regressions in time or allocations are visible in review.

**Why this priority**: The value of the measurements comes from comparing them over time.

**Independent Test**: Follow the README to run the benchmarks and compare against the recorded
baseline; differences per scenario are readable side by side.

**Acceptance Scenarios**:

1. **Given** the repository, **When** a reader opens the benchmarks folder, **Then** they find a
   baseline for the current core version with the environment it was measured on.
2. **Given** a code change, **When** the maintainer follows the README comparison steps, **Then**
   they can see per-scenario differences in time and allocations against the baseline.

---

### Edge Cases

- **Different machines**: absolute times differ across machines; the README states that comparisons
  are only meaningful on the same machine and that allocations are the more stable signal.
- **Debug build**: running without an optimized build is refused or warned against, as documented.
- **Scenario relying on default title**: the "no title" scenarios exercise the reason-phrase default
  (feature 002); "with title" scenarios exercise the explicit path.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The repository MUST provide on-demand benchmarks for these core scenarios:
  - creating a success and a failure result, without and with a value;
  - implicit conversions (value → typed result, untyped failure → typed result);
  - creating a problem directly and through the builder, with and without a title;
  - writing a problem to JSON and reading it back, with and without extension members.
- **FR-002**: Each scenario MUST report mean time and allocated memory per operation.
- **FR-003**: Benchmarks MUST be selectable by group or name through a documented filter.
- **FR-004**: Benchmarks MUST NOT run as part of the normal test run or CI by default, and MUST NOT be
  packed or published.
- **FR-005**: The benchmarks project MUST follow the repository build rules (warnings as errors,
  nullable enabled, vulnerability audit).
- **FR-006**: A README in the benchmarks folder MUST explain how to run all or some benchmarks, how to
  compare against the baseline, and the limits of comparisons across machines.
- **FR-007**: A baseline for the current core version MUST be recorded in the repository, including
  the environment (OS, CPU, runtime) it was measured on.
- **FR-008**: Benchmarks MUST measure the published core package code as-is (no benchmark-only
  changes to the core).

### Key Entities

- **Benchmark scenario**: A named operation of the core package, grouped by area (results, problems,
  JSON).
- **Baseline**: A recorded report of all scenarios for a given core version and environment.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of the scenarios listed in FR-001 appear in the baseline with time and allocations.
- **SC-002**: A new contributor can run the benchmarks and compare against the baseline using only the
  README, in under 10 minutes of hands-on time (excluding benchmark run time).
- **SC-003**: The normal build and test run time does not increase by more than the time to compile
  the benchmarks project (no benchmark is executed).
- **SC-004**: 0 build warnings in the benchmarks project.

## Assumptions

- Benchmarks target the newest supported runtime (net10.0) only; other target frameworks of the core
  are covered by tests, not benchmarks.
- CI does not run benchmarks; an optional manual workflow may be added later in a separate feature.
- The baseline is stored as a readable report (Markdown) in the benchmarks folder and refreshed when
  a change intentionally alters performance.
- Core package code is not changed by this feature.
- Constitution Principle V also lists ASP.NET Core result conversion and mediator dispatch as hot
  paths. Their benchmarks are out of scope here and will be added in later features: Mediators
  before its next hot-path change, ASP.NET Core together with `Viana.Results.AspNetCore`.
- Work happens on the shared branch `chore/add-spec-kit`.
