# Research: Warnings as Errors

**Feature**: [spec.md](./spec.md) | **Date**: 2026-11-21

## Baseline (measured 2026-11-21, .NET SDK 10.0.401, `dotnet build Viana.Results.slnx -c Release`)

| Warning | Where | Count |
|---|---|---|
| NU1903 (high severity vulnerability, GHSA-v5pm-xwqc-g5wc) on `Microsoft.OpenApi` 2.0.0 / 2.4.1 (transitive) | `src/Viana.Results.OpenApi`, `src/Viana.Results.OpenApi.Swashbuckle`, `tests/Viana.Results.OpenApi.Tests`, all 3 `examples/` | only warning in the build |

Hidden (suppressed) warnings found:

| Suppression | Where | Effect |
|---|---|---|
| `<Nullable>annotations</Nullable>` | all `tests/*.Tests.csproj` except Mvc.Tests | Nullable warnings are not emitted at all. Enabling them yields 5 warnings (CS8600 ×4, CS8604 ×1) in `Viana.Results.Tests` and `Viana.Results.OpenApi.Tests`. |
| `<NoWarn>$(NoWarn);CS1591</NoWarn>` | `examples/Viana.Results.Examples.Shared` | Missing XML docs allowed in examples. |
| Null-forgiving `title!` / `type!` | `src/Viana.Results/ProblemResultJsonConverter.cs:64-65` | Hides that a deserialized problem without `title`/`type` passes `null` into non-nullable parameters. |

## R1. Where to define the rule

- **Decision**: A new `Directory.Build.props` at the repository root sets
  `TreatWarningsAsErrors=true` (plus `Nullable=enable`) for every project in the repo.
- **Rationale**: MSBuild imports it automatically into every project under the root, so new
  projects inherit the rule with zero setup (FR-003, SC-002). Local builds and CI use the same file
  (FR-007).
- **Alternatives considered**: Per-csproj property (rejected: easy to forget on new projects);
  `-warnaserror` only in CI (rejected: violates FR-007, local and CI would diverge).

## R2. Do NuGet audit warnings become errors?

- **Decision**: Yes. `TreatWarningsAsErrors` also promotes NuGet restore warnings (NU1901–NU1904),
  so vulnerability advisories fail `dotnet restore`/`build` locally and in CI, as clarified
  (FR-009). Audit mode stays at the SDK default (`all`, which includes transitive packages on the
  .NET 10 SDK); it is set explicitly (`NuGetAuditMode=all`) so the behavior does not depend on SDK
  defaults.
- **Rationale**: The vulnerable package is transitive, so direct-only auditing would miss it.
- **Alternatives considered**: `WarningsNotAsErrors` for NU190x (rejected by clarification A).

## R3. Fixing the vulnerable `Microsoft.OpenApi`

- **Findings**:
  - `Microsoft.AspNetCore.OpenApi` 10.0.3 depends on `Microsoft.OpenApi` 2.0.0;
    `Swashbuckle.AspNetCore` 10.1.4 depends on 2.4.1. No project references `Microsoft.OpenApi`
    directly, so the `3.3.1` entry in `Directory.Packages.props` is currently unused.
  - Audit check per version (scratch restore, net10.0): 2.4.3 vulnerable; 2.7.6 clean;
    **2.13.1 clean**; **3.3.1 vulnerable**; 3.11.1 clean.
- **Decision**: Enable `CentralPackageTransitivePinningEnabled=true` and pin
  `Microsoft.OpenApi` to **2.13.1** (latest 2.x) in `Directory.Packages.props`.
- **Rationale**: Both upstream packages are compiled against the 2.x major; moving them to 3.x
  risks binary breaking changes at runtime. Latest 2.x is clean and minor-compatible. The unused
  3.3.1 entry is itself vulnerable and must not become active through pinning.
- **Consequence**: With transitive pinning, the published `Viana.Results.OpenApi` and
  `Viana.Results.OpenApi.Swashbuckle` packages declare `Microsoft.OpenApi >= 2.13.1`, which also
  protects consumers from resolving the vulnerable version. Not a breaking change.
- **Alternatives considered**: 3.11.1 (rejected: major mismatch with upstream);
  direct `PackageReference` in each project (rejected: duplicates intent, transitive pinning is
  the CPM-native way); upgrading `Microsoft.AspNetCore.OpenApi` / `Swashbuckle` (preferred if a
  newer upstream already depends on a clean version; to be checked at implementation time, and
  the pin removed if no longer needed).

- **Outcome (implementation, 2026-11-21)**: The preferred alternative applied.
  `Microsoft.AspNetCore.OpenApi` 10.0.15 and `Swashbuckle.AspNetCore` 10.3.1 resolve
  `Microsoft.OpenApi` 2.13.0 (clean), so they were bumped and no transitive pin was added. The
  unused, vulnerable `Microsoft.OpenApi` 3.3.1 entry was removed from `Directory.Packages.props`.

- **Additional findings (implementation)**: Switching `NuGetAuditMode` to `all` surfaced three more
  high-severity transitive vulnerabilities in test projects that the default audit did not report:
  `System.Text.RegularExpressions` 4.3.0 (GHSA-cmhx-cq75-c4mj, via `xunit` 2.5.3 →
  `NETStandard.Library` 1.6.1) and `System.Text.Json` 8.0.0 (GHSA-8g4q-xg66-9fp4,
  GHSA-hh2w-p6rv-4g7w, via `Microsoft.AspNetCore.Mvc.Testing` 8.0.0). Fixed by bumping `xunit` to
  2.9.3 and `Microsoft.AspNetCore.Mvc.Testing` to 8.0.34 (latest 8.0.x, matching the test TFM).

## R4. Tests: `Nullable=annotations`

- **Decision**: Remove the per-project overrides; tests inherit `Nullable=enable` from
  `Directory.Build.props`; fix the 5 resulting warnings in test code.
- **Rationale**: `annotations` silently disables nullable warnings — a suppression without
  justification (FR-002, FR-005). The cost is 5 small fixes.

## R5. Null-forgiving operators in `ProblemResultJsonConverter`

- **Finding (corrected during implementation)**: The `!` does not hide a real null flow. The
  converter already defaults `title` to `"Error"` and `type` to `"about:blank"` before using them;
  the `!` exists only because `string.IsNullOrWhiteSpace` has no nullability annotations on
  `netstandard2.0`. The `ProblemResult` constructor applies the same defaults.
- **Decision**: Pass `title ?? string.Empty` / `type ?? string.Empty` to the constructor and let it
  apply its existing defaults; remove the duplicated defaulting in the converter. No behavior
  change.
- **Deferred**: Defaulting `title` to the status reason phrase (RFC 9457 §4.2.1) changes tested,
  documented behavior in `ProblemResult`, `ProblemBuilder` and the converter; it becomes its own
  feature (maintainer decision, 2026-11-21).

## R5b. Null-forgiving audit in `src/` (T020)

| Location | Classification | Action |
|---|---|---|
| `Results.cs:97,100`, `ResultTypes.cs:81` | `!` after `string.IsNullOrEmpty/IsNullOrWhiteSpace`: no nullability annotations on `netstandard2.0` | Allowed pattern (documented in contracts/build-policy.md) |
| `ProblemResultJsonConverter.cs:38` (`reader.GetString()!` on a `PropertyName` token) | JSON property names are never null | Allowed |
| `Mediator.cs:100,114,120` | `GetMethod("Handle")` guaranteed by `IHandler<,>`; `Invoke` of a `Task<T>`-returning method; `default!` after `ExceptionDispatchInfo.Throw()` (unreachable) | Allowed |
| `Result.cs:112` (`result!.Status`) | Compensated a redundant `?.` on line 110 (`result` is a non-nullable parameter already dereferenced on line 105) | **Removed** both the `?.` and the `!`; no behavior change |
| `OpenApiOperationProcessor.cs:131` (`response.Content!.Values`) | Initially flagged as a latent bug; **corrected**: the only (private) caller already skips responses with null `Content`, so null never reaches it — the compiler just cannot see the caller's check | **Refactored**: the caller passes the null-checked `Content` dictionary (`ApplyExamplesToContent`); `!` removed, no behavior change |

Tests use `null!` to exercise argument validation and `Problem!` after asserting failure; both are
idiomatic and allowed in test code.

## R6. Examples: CS1591

- **Decision**: Keep `CS1591` suppressed only in example projects, moved into a single
  `examples/Directory.Build.props` (imports the root one) with an inline XML comment justifying
  it: examples are not published and exist to be read, not consumed as API.
- **Rationale**: Narrowest scope that still applies to all examples (FR-005).

## R7. CI

- **Decision**: No new CI step is required for enforcement: `dotnet restore` and `dotnet build`
  in `.github/workflows/ci.yml` fail on any warning because the rule lives in
  `Directory.Build.props`. No extra SDKs are needed in `setup-dotnet`: the 10.x SDK builds all
  TFMs (including netstandard2.0 and net6.0) via targeting packs, as verified by the local build.
- **Rationale**: Single source of truth; CI = local (FR-006, FR-007).

## R8. Contributor documentation

- **Decision**: Add a short "Zero warnings" section to `README.md` (Contributing) stating the rule
  and that any suppression needs an inline justification and reviewer approval (FR-008).
