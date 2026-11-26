# Tasks: Clean Up Unused Central Package Entries

**Input**: Design documents from `specs/003-cleanup-central-packages/`

**Tests**: No new tests (no behavior change); the existing suite is the regression gate.

## Phase 1: Setup

- [X] T001 Record baseline: run `dotnet build -c Release` and `dotnet test -c Release` from the repository root (expect 0 warnings, all tests passing)

## Phase 2: Foundational

- [X] T002 Re-verify that Microsoft.AspNet.Mvc, Microsoft.AspNetCore.Mvc.Core, Newtonsoft.Json and Swashbuckle.AspNetCore.SwaggerGen have no `PackageReference`/`GlobalPackageReference` in any `*.csproj`, `*.props` or `*.targets` under src/, tests/, examples/ or the root (excluding bin/obj); stop if any is referenced

## Phase 3: User Story 1 – Central list reflects real usage (P1) 🎯 MVP

**Goal**: Directory.Packages.props declares only used packages.

**Independent Test**: Every remaining `PackageVersion` in Directory.Packages.props has at least one `PackageReference` in a project; build and tests pass.

- [X] T003 [US1] In Directory.Packages.props remove the `PackageVersion` entries Microsoft.AspNet.Mvc (5.3.0), Microsoft.AspNetCore.Mvc.Core (2.3.9), Newtonsoft.Json (13.0.4) and Swashbuckle.AspNetCore.SwaggerGen (8.0.0), and the empty `<ItemGroup>`; keep the other 12 entries and their versions unchanged
- [X] T004 [US1] Check every remaining `PackageVersion` in Directory.Packages.props is referenced by at least one `*.csproj` (quickstart.md step 2)

## Phase 4: Polish

- [X] T005 Run specs/003-cleanup-central-packages/quickstart.md steps 3–6: `dotnet restore` (no NU1xxx), `dotnet build -c Release` (0 warnings, 0 errors), `dotnet test -c Release` (all pass), `git diff --stat` shows only Directory.Packages.props outside specs/

## Dependencies & Execution Order

T001 → T002 → T003 → T004 → T005 (strictly sequential; single file, no parallelism)

## Implementation Strategy

Single increment: US1 is the whole feature and the MVP.
