---

description: "Task list for Warnings as Errors"
---

# Tasks: Warnings as Errors

**Input**: Design documents from `specs/001-warnings-as-errors/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md),
[data-model.md](./data-model.md), [contracts/build-policy.md](./contracts/build-policy.md)

**Tests**: Included. The constitution makes TDD mandatory (Principle III): the converter change
(R5) is guarded by characterization tests written first; the build rule itself is verified with deliberate-warning checks
from [quickstart.md](./quickstart.md).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Record the baseline before changing anything.

- [X] T001 Record baseline: run `dotnet build Viana.Results.slnx -c Release --no-incremental` and `dotnet list Viana.Results.slnx package --vulnerable --include-transitive`; confirm only NU1903 on `Microsoft.OpenApi` 2.0.0/2.4.1 appears (matches research.md Baseline)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Clear existing warnings that would break the build the moment the rule is enabled.

**⚠️ CRITICAL**: Enabling warnings as errors (US1) before this phase would break restore/build.

- [X] T002 Enable `CentralPackageTransitivePinningEnabled=true` and change `Microsoft.OpenApi` from `3.3.1` (vulnerable, unused) to `2.13.1` in Directory.Packages.props (research R3) — **superseded by T003**: pin not needed; the unused vulnerable `Microsoft.OpenApi` 3.3.1 entry was removed instead
- [X] T003 Check whether a newer `Microsoft.AspNetCore.OpenApi` 10.0.x / `Swashbuckle.AspNetCore` 10.x already depends on a clean `Microsoft.OpenApi`; if so, bump them in Directory.Packages.props and note whether the pin from T002 is still needed (research R3) — bumped `Microsoft.AspNetCore.OpenApi` 10.0.3 → 10.0.15 and `Swashbuckle.AspNetCore` 10.1.4 → 10.3.1; both resolve clean `Microsoft.OpenApi` 2.13.0
- [X] T004 Verify `dotnet list Viana.Results.slnx package --vulnerable --include-transitive` reports no vulnerable packages and `dotnet build` has 0 NU1903 warnings

**Checkpoint**: Build has zero warnings with current settings; the rule can be turned on.

---

## Phase 3: User Story 1 - Published packages build with zero warnings (Priority: P1) 🎯 MVP

**Goal**: Every `src/` package fails its build on any warning, for every TFM, with the rule defined once.

**Independent Test**: Add `public static int Probe;` to src/Viana.Results/Results.cs and build → `error CS1591`; revert → build succeeds with 0 warnings (quickstart §1–2).

### Tests for User Story 1 ⚠️

> Write these tests FIRST, ensure they FAIL before implementation.

- [X] T005 [P] [US1] (FR-010) Add characterization tests in tests/Viana.Results.Tests/ProblemResultJsonConverterTests.cs locking current behavior: a problem JSON with `type`/`title` absent (not just empty) yields `Type == "about:blank"` and `Title == "Error"`; with both absent, both defaults apply (research R5)

### Implementation for User Story 1

- [X] T006 [US1] Create Directory.Build.props at the repository root with `TreatWarningsAsErrors=true`, `Nullable=enable` and `NuGetAuditMode=all` (research R1, R2)
- [X] T007 [US1] Remove `title!` / `type!` in src/Viana.Results/ProblemResultJsonConverter.cs by passing `title ?? string.Empty` / `type ?? string.Empty` and relying on the `ProblemResult` constructor defaults (remove the duplicated defaulting); T005 and all existing tests must still pass (research R5)
- [X] T008 [US1] Remove now-redundant `<Nullable>enable</Nullable>` from src/*/\*.csproj and examples/*/\*.csproj, since Directory.Build.props provides it (keep `LangVersion`, `GenerateDocumentationFile`)
- [X] T009 [US1] Build all `src/` packages for every TFM (`dotnet build Viana.Results.slnx -c Release --no-incremental`) and fix any warning at its source until 0 warnings

**Checkpoint**: All published packages build warning-free and fail on any new warning.

---

## Phase 4: User Story 2 - CI blocks any new warning (Priority: P2)

**Goal**: CI fails on any warning, using the same rule as local builds.

**Independent Test**: A PR with a deliberate warning fails the `CI` workflow; without it, CI passes (quickstart §6).

### Implementation for User Story 2

- [X] T010 [US2] Confirm .github/workflows/ci.yml needs no change: `dotnet restore` and `dotnet build` pick up Directory.Build.props; verify no step passes `-warnaserror-`, `-p:TreatWarningsAsErrors=false` or `NoWarn` overrides (research R7)
- [X] T011 [US2] Simulate CI in a fresh clone (never `git clean` the working copy): after committing, `git clone . <scratchpad>/ci-sim`, then inside it run `dotnet restore Viana.Results.slnx`, `dotnet build Viana.Results.slnx --no-restore -c Release`, `dotnet test Viana.Results.slnx --no-build -c Release`; all must pass with 0 warnings; delete the clone afterwards

**Checkpoint**: CI and local builds enforce the same zero-warning rule.

---

## Phase 5: User Story 3 - Tests and examples follow the same rule (Priority: P3)

**Goal**: Test and example projects are covered by the rule; the only exemption is CS1591 in examples, justified inline.

**Independent Test**: `string s = null;` in a test project and in an example project → `error CS8600`; an undocumented public member in an example → builds (quickstart §3).

### Implementation for User Story 3

- [X] T012 [P] [US3] Remove `<Nullable>annotations</Nullable>` from tests/Viana.Results.Tests/Viana.Results.Tests.csproj, tests/Viana.Results.OpenApi.Tests/Viana.Results.OpenApi.Tests.csproj and tests/Viana.Results.Mediators.Tests/Viana.Results.Mediators.Tests.csproj; remove the redundant `<Nullable>enable</Nullable>` from tests/Viana.Results.Mvc.Tests/Viana.Results.Mvc.Tests.csproj (research R4)
- [X] T013 [P] [US3] Fix CS8600 in tests/Viana.Results.Tests/ResultGenericTests.cs:136 and tests/Viana.Results.Tests/ResultsHelperTests.cs:145 (use nullable locals or assertions, not `!`)
- [X] T014 [P] [US3] Fix CS8600 in tests/Viana.Results.OpenApi.Tests/ProblemResultAttributeTests.cs:42 and CS8604 in tests/Viana.Results.OpenApi.Tests/ProblemResultSchemaTests.cs:55,64 (use nullable locals or assertions, not `!`)
- [X] T015 [US3] Create examples/Directory.Build.props that imports the root Directory.Build.props and adds `<NoWarn>$(NoWarn);CS1591</NoWarn>` with an inline XML comment: "Examples are not published and are not consumed as API. The XML docs file is still generated to feed OpenAPI descriptions, but full documentation coverage is not required." (research R6, data-model Justified Exception)
- [X] T016 [US3] Remove the local `<NoWarn>$(NoWarn);CS1591</NoWarn>` from examples/Viana.Results.Examples.Shared/Viana.Results.Examples.Shared.csproj
- [X] T017 [US3] Build the whole solution and fix any remaining warning in tests/ and examples/ at its source until 0 warnings

**Checkpoint**: Every project in the repository is covered; SC-001 and SC-002 hold.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T018 [P] Add a "Zero warnings" subsection to the Contributing part of README.md: rule summary, vulnerable dependencies fixed by upgrading/pinning in Directory.Packages.props, and suppressions need a single code, narrowest scope, inline justification and reviewer approval (FR-008, contracts/build-policy.md)
- [X] T019 [P] Add release notes (`PackageReleaseNotes`) in src/Viana.Results.OpenApi/Viana.Results.OpenApi.csproj and src/Viana.Results.OpenApi.Swashbuckle/Viana.Results.OpenApi.Swashbuckle.csproj: dependency bumps (`Microsoft.AspNetCore.OpenApi` 10.0.15, `Swashbuckle.AspNetCore` 10.3.1) that resolve the vulnerable transitive `Microsoft.OpenApi` (GHSA-v5pm-xwqc-g5wc) (manual for now; becomes commit-derived once the automatic versioning feature lands)
- [X] T020 Audit all suppressions: search for `NoWarn`, `#pragma warning disable`, `SuppressMessage` and null-forgiving `!` used to silence warnings across src/, tests/, examples/; each must have an inline justification (SC-005)
- [X] T021 Run every scenario in specs/001-warnings-as-errors/quickstart.md and confirm expected outcomes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: none
- **Foundational (Phase 2)**: after Setup — BLOCKS all user stories (vulnerability would fail restore once US1 is on)
- **US1 (Phase 3)**: after Foundational
- **US2 (Phase 4)**: after US1 (needs Directory.Build.props from T006)
- **US3 (Phase 5)**: after US1 (T006 turns on `Nullable=enable` for tests/examples). Note: right after T006 the solution build fails until T012–T016 are done; do US1 and US3 back-to-back before committing
- **Polish (Phase 6)**: after all stories

### Within User Story 1

- T005 (failing tests) → T006 → T007 (make tests pass) → T008 → T009

### Parallel Opportunities

- T005 can be written in parallel with T002–T004
- T012, T013, T014 touch different files and can run in parallel
- T018, T019 can run in parallel

---

## Parallel Example: User Story 3

```bash
Task: "Remove Nullable=annotations from test csproj files (T012)"
Task: "Fix CS8600 in tests/Viana.Results.Tests (T013)"
Task: "Fix CS8600/CS8604 in tests/Viana.Results.OpenApi.Tests (T014)"
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Phase 1 → Phase 2 (vulnerability fixed, build still green)
2. Phase 3 (US1): rule on, converter fixed
3. Immediately Phase 5 (US3) so the whole solution builds again
4. **VALIDATE**: quickstart §1–3

### Incremental Delivery

1. Commit after Phase 2: `fix(openapi): pin Microsoft.OpenApi to 2.13.1 to resolve GHSA-v5pm-xwqc-g5wc`
2. Commit after US1 + US3: `build: treat warnings as errors across the repository`
   (converter fix may be its own commit: `fix(core): apply RFC 9457 defaults for missing title/type`)
3. Commit after US2/Polish: `docs: document zero-warnings rule`

---

## Notes

- [P] tasks = different files, no dependencies
- Never fix a warning with `!`, `#pragma` or `NoWarn` without an inline justification
- Commit after each task or logical group, using Conventional Commits
