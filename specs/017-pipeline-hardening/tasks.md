# Tasks: Pipeline hardening

**Input**: [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [quickstart.md](quickstart.md)

**Tests**: each guard is proven by a deliberate failing change (quickstart "Guards"). The C# AOT fixes are
test-first: the smoke app and the AOT build are the failing test. Before relying on any research item marked ⚠️,
check it against its current documentation and note the result next to the task.

## Phase 1: Setup

- [X] T001 Confirm that every released baseline exists on nuget.org: Viana.Results 2.0.0, Viana.Results.Mvc 2.0.0,
      Viana.Results.Mediators 1.0.0, Viana.Results.OpenApi 1.0.0 and Viana.Results.OpenApi.Swashbuckle 1.1.0. Query
      the public package endpoint for each id, and only after the user approves sending the ids. Record the result
      under "Baselines". If any is missing, stop and ask the user before configuring package validation (R7).
- [X] T002 [P] Add `global.json` pinning the installed 10.0 SDK, with `rollForward: latestPatch` (R6).
- [X] T003 [P] Add `nuget.config` at the root: `<clear/>` the sources, add nuget.org, and add a
      `<packageSourceMapping>` that maps `*` to nuget.org (R2).

## Phase 2: Foundational (blocks all stories)

- [X] T004 In `Directory.Build.props`, add `RestorePackagesWithLockFile=true`. Generate the `packages.lock.json`
      files with `dotnet restore Viana.Results.slnx --force-evaluate`, then confirm that
      `dotnet restore --locked-mode` passes and that a Release build has 0 warnings (R1).
- [X] T005 Format the repository once with `dotnet format Viana.Results.slnx`, as its own commit. Confirm that
      `dotnet format --verify-no-changes` passes and that the tests are green (R8).

**Checkpoint**: locked restore, formatted tree, green build.

## Phase 3: User Story 1, dependencies can be trusted (P1) 🎯 MVP

- [X] T006 [P] [US1] Create `.github/dependabot.yml` (R3):
      - ecosystems `nuget` and `github-actions`, both with `directory: "/"`, `schedule.interval: weekly`,
        `cooldown.default-days: 7`, and groups for minor and patch updates;
      - confirm ⚠️ that Dependabot updates `Directory.Packages.props` and the lock files, and note the result.
        **Note (2026-11-26):** the supported-ecosystems page only says NuGet "supports most features up until
        version 6.8.0"; it does not state CPM or lock-file support. Verify on the first Dependabot PR.
- [X] T007 [P] [US1] Create `.github/workflows/dependency-review.yml` (R4): on `pull_request`, with
      `permissions: contents: read, pull-requests: write`, and `actions/dependency-review-action` configured with
      `fail-on-severity: moderate`, `allow-licenses: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD` (D2)
      and `comment-summary-in-pr: on-failure`. Pin the action by SHA.
- [X] T008 [US1] Rewrite `.github/workflows/ci.yml` for the supply-chain part (R1, R5, R6):
      - triggers: push and pull_request on the release branch (`main`) and on `dev`;
      - build and tests in a matrix of `ubuntu-latest` and `windows-latest`, with `fail-fast: false`;
      - top-level `permissions: contents: read`, `concurrency` per ref, and job timeouts;
      - `actions/checkout` and `actions/setup-dotnet` pinned by SHA, with `global-json-file` plus the 8.0 runtime;
      - `dotnet restore --locked-mode`, Release build and tests.
- [X] T009 [US1] Pin every action in `.github/workflows/release.yml` by SHA, and add top-level
      `permissions: contents: read`. The full rewrite comes in US3.

**Independent test (quickstart guards)**: an unlocked transitive change fails the restore; a vulnerable package in
a PR fails the dependency review; no workflow references an action by tag.

## Phase 4: User Story 2, broken code does not merge (P1)

- [X] T010 [US2] Create `tests/Viana.Results.AotSmoke/` and add it to `Viana.Results.slnx`. It is a net10.0
      console app with `PublishAot=true`, `TreatWarningsAsErrors=true` and `RestorePackagesWithLockFile=false`, and
      it references `Viana.Results`. It exercises results, `ProblemResult` JSON (converter read and write), list
      and paged bodies, and envelope `GetBody(result, options)` through a source-generated `JsonSerializerContext`.
      Mark `src/Viana.Results/Viana.Results.csproj` with `IsAotCompatible` for TFMs compatible with net8.0. Confirm
      that `dotnet publish -r linux-x64` (WSL or CI) or the AOT analyzers fail on the 6 pre-existing warnings. This
      is the failing test.
- [X] T011 [US2] Fix the core AOT warnings (D1):
      - `src/Viana.Results/ProblemResultJsonConverter.cs`, lines 126 and 158: replace the reflection-only
        `JsonSerializer.Deserialize<T>`/`Serialize(object)` calls with `JsonTypeInfo`-based calls from the options;
      - `src/Viana.Results/ResultReflections.cs`, `IsUnwrappableType`/`GetDataType`: proper
        `DynamicallyAccessedMembers` annotations, or narrow justified suppressions like `IsEnvelopeType`.

      Make T010 pass, keep the full suite green, and rerun the Mvc/AspNetCore benchmarks against
      `baseline/envelope-after.md`, recording the result under "Benchmark".
- [X] T012 [US2] In `Directory.Build.props`, enable `EnablePackageValidation` for packable projects (R7). In
      `ci.yml`, add a `pack` job: for each package, read the latest `<component>-v*` tag, run
      `dotnet pack -p:PackageValidationBaselineVersion=<x>`, and skip the baseline for packages without a tag
      (AspNetCore).
- [X] T013 [US2] Run the T012 validation locally. Generate `CompatibilitySuppressions.xml` for each package whose
      intended breaks from the 2026-11-26 audit appear, with `-p:ApiCompatGenerateSuppressionFile=true`. Commit the
      files, and list the suppressed diagnostics under "Intended breaks" so the user can review them.
- [X] T014 [P] [US2] In `ci.yml`, add a `format` job (`dotnet format --verify-no-changes`) and an `aot-smoke` job
      (publish `tests/Viana.Results.AotSmoke` for `linux-x64` and run the binary).
- [X] T015 [P] [US2] Create `.github/workflows/codeql.yml` (R9): `csharp` with `build-mode: manual`, the pinned SDK
      plus `dotnet build`, on PR, on push to the release branch and weekly, with `permissions: security-events: write,
      contents: read` and actions pinned by SHA. Confirm the ⚠️ build-mode guidance.

**Independent test**: the quickstart guards for formatting, a removed public member and AOT each fail CI.

## Phase 5: User Story 3, safe publishing and automatic versions (P2)

- [X] T016 [US3] This is the last manual `<Version>` edit: from here on release-please owns it. Reset `<Version>`
      in every `src/*/*.csproj` to the last released tag: core 2.0.0, mvc 2.0.0,
      mediators 1.0.0, openapi 1.0.0 and openapi-swashbuckle 1.1.0. For AspNetCore, keep 1.0.0, since it has
      never been released. Seed `src/<Project>/CHANGELOG.md` from each `<PackageReleaseNotes>`, and replace
      `<PackageReleaseNotes>` with a link to that CHANGELOG on GitHub (D4).
- [X] T017 [US3] Create `release-please-config.json` and `.release-please-manifest.json` (R11):
      - one package per `src/<Project>`, with `release-type: simple` and `component` set to `core`, `mediators`,
        `mvc`, `openapi`, `openapi-swashbuckle` or `aspnetcore`;
      - `extra-files: [{ "type": "xml", "path": "<Project>.csproj", "xpath": "//Project/PropertyGroup/Version" }]`;
      - the manifest is seeded with the T016 versions, and AspNetCore gets `release-as: 1.0.0`;
      - confirm ⚠️ the action outputs and the `simple`/`version.txt` behavior.
- [X] T018 [US3] Create `.github/workflows/release-please.yml`: on push to the release branch, with
      `permissions: contents: read`. A step `actions/create-github-app-token` (pinned by SHA) uses the secrets
      `RELEASE_APP_ID` and `RELEASE_APP_PRIVATE_KEY` to get an installation token. That token is passed to
      `googleapis/release-please-action@v4` (pinned by SHA) with the config and manifest. With it, the tags it
      creates trigger `release.yml`, which `GITHUB_TOKEN` events do not (U1, option b). Confirm ⚠️ both actions'
      inputs.
- [X] T019 [US3] Rewrite `.github/workflows/release.yml` (R12), keeping the file name for the Trusted Publishing
      policy:
      - **Trigger**: the release tags (`core-v*` … `openapi-swashbuckle-v*`, plus `aspnetcore-v*`).
      - **`verify` job** (contents: read):
        - check the ancestry with `git merge-base --is-ancestor $GITHUB_SHA origin/<release branch>`;
        - check that the tag version equals `<Version>`;
        - locked restore, build, tests;
        - pack with package validation;
        - SBOM generation;
        - upload the artifacts.
      - **`publish` job**:
        - `environment: nuget`, with `permissions: id-token: write, attestations: write, contents: write`;
        - `actions/attest-build-provenance` and `actions/attest-sbom` on the `.nupkg` files;
        - `NuGet/login@v1` (`user: ${{ secrets.NUGET_USER }}`), right before
          `dotnet nuget push --api-key ${{ steps.login.outputs.NUGET_API_KEY }}`;
        - upload the `.nupkg`, `.snupkg` and SBOM to the release that release-please created.
      - **Removed**: the old `NUGET_API_KEY` secret and the self-created GitHub release.
      - Confirm ⚠️ the attestation action versions and inputs.

## Phase 6: User Story 4, settings checklist (P3)

- [X] T020 [US4] Create `RELEASING.md` (R13):
      - **Flow**: Conventional Commits per package path; release PRs; merging creates the tag; the publish waits
        for approval; how to ship a dependency-only fix (`Release-As`, D3); how an intended break is recorded
        (`!` commit plus `CompatibilitySuppressions.xml`).
      - **Settings checklist**: repository rulesets for the release branch (required checks for ci, codeql and
        dependency-review; reviews; no force push); a tag ruleset for `*-v*`; secret scanning with push
        protection; default workflow permissions set to read; the `nuget` environment with a required reviewer;
        the nuget.org Trusted Publishing policy (owner, repo, `release.yml`, environment `nuget`); the GitHub App
        for release-please (installed on this repository only, contents and pull-requests write) and its secrets
        `RELEASE_APP_ID` and `RELEASE_APP_PRIVATE_KEY`.
      - **Order** (so publishing never breaks):
        1. create the `NUGET_USER` secret and the Trusted Publishing policy;
        2. create the `nuget` environment;
        3. create the GitHub App and its secrets;
        4. publish one release with the new flow;
        5. only then revoke the old NuGet API key and delete the `NUGET_API_KEY` secret.

## Phase 7: Polish

- [X] T021 Run the quickstart:
      - local commands;
      - the guards that run locally, on a scratch branch, reverting each change and recording the results under
        "Guards": locked restore, formatting, package validation and AOT;
      - list the guards that need GitHub (dependency review, Dependabot cooldown, the release dry run). The user
        validates them on a test PR, or I run them only with the user's explicit authorization to push;
      - grep that 0 actions are referenced by tag, and that no workflow or doc names another mediator library
        (FR-013).

      Set the spec status to Ready.

## Phase 8: Fixes found on the first CI run (2026-11-26)

- [X] T022 In `.github/workflows/ci.yml` (job `pack`) and `.github/workflows/release.yml` (step "Pack (with package
      validation)"), add `-p:GeneratePackageOnBuild=false` to `dotnet pack`. The csproj files set
      `GeneratePackageOnBuild=true`. Combined with a `dotnet pack` that builds a multi-target project from a clean
      checkout, this packs before the TFM outputs exist (NU5026 `bin/Release/net10.0/Viana.Results.dll` not
      found). It passed locally only because `bin/` already existed. Reproduced and verified locally from a clean
      `bin/obj`.
- [X] T023 Dependency review fails with `Forbidden` (dependency-graph API). This is a repository setting, not a
      file: enable Settings → Code security → Dependency graph. Add this as the first item of the security step in
      `RELEASING.md`, and note that the workflow needs it.

## Phase 9: dev branch flow and unblocking the first release (2026-11-26)

Context: PR #1 was squash-merged into `main` with a non-conventional title, so release-please found no
releasable commit and opened no release PR. The user wants to keep squash merges for feature PRs, so the flow
becomes: feature → squash → `dev` (PR title must be a Conventional Commit) → **merge commit** → `main` →
release-please. Versions for the first release were approved by the user: core 3.0.0, mvc 3.0.0, mediators
2.0.0, openapi 2.0.0, openapi-swashbuckle 2.0.0, aspnetcore 1.0.0.

The work starts from `origin/main` (`d5bd902`). The branch name and the push are confirmed with the user first.

- [X] T024 In `release-please-config.json`, add a temporary `"release-as"` to each package with the approved
      versions (core 3.0.0, mvc 3.0.0, mediators 2.0.0, openapi 2.0.0, openapi-swashbuckle 2.0.0; aspnetcore
      already has 1.0.0).
- [X] T025 In each `src/<Project>/CHANGELOG.md`, replace the "Unreleased" seed with a short summary of the
      changes and the breaking changes for that release (from the 2026-11-26 audit and the package-validation
      list). This also gives the unblocking commit a change inside every package path. Do not name any other
      mediator library.
- [X] T026 Create `.github/workflows/pr-title.yml`, which checks that the titles of PRs targeting `dev` are
      Conventional Commits:
      - use `amannn/action-semantic-pull-request` pinned by SHA (a release at least 7 days old), with
        `permissions: pull-requests: read`;
      - allowed types: feat, fix, docs, chore, ci, refactor, perf, test, style, build, revert, with an optional
        scope and `!`;
      - confirm ⚠️ the action's inputs against its `action.yml`.
- [X] T027 Update `RELEASING.md` for the dev flow:
      - feature branches → PR to `dev` → **squash**, with the PR title as a Conventional Commit, checked by
        `pr-title`;
      - `dev` → PR to `main` → **merge commit** (never squash), with a free title;
      - release-please runs on `main`.

      Add the settings:
      - create `dev` from `main`;
      - ruleset for `dev`: require a PR, the CI checks and `pr-title`, and allow only squash merges;
      - ruleset for `main`: allow only merge commits, and accept only PRs from `dev`;
      - make `dev` the default branch.

      Note how the first release was unblocked (T024) and that the `release-as` entries are removed right after
      it (T029).
- [X] T028 Run the local checks (Release build with 0 warnings, tests, `dotnet format --verify-no-changes`), and
      tell the user how to merge the unblocking PR into `main`: with a `feat!:` title (squash is acceptable for
      this single PR because its title is conventional).
- [ ] T029 After the first release PRs are merged and published, remove every `"release-as"` from
      `release-please-config.json`, so the following versions come from the commits again. This is a follow-up PR
      through `dev`.

## Phase 10: Dependabot follow-ups (2026-11-26)

The first Dependabot run confirmed that it updates `Directory.Packages.props` and the lock files, and that the
7-day cooldown works. It also showed three gaps:

- it did not regenerate `benchmarks/Viana.Results.Benchmarks/packages.lock.json`, so PR #2 failed with NU1004
  (reproduced locally);
- it proposed major bumps of library-facing packages (#4 and #5: `Microsoft.Extensions.DependencyInjection(.Abstractions)`
  8 → 11), which would force consumers of Mediators on net6/net8 onto the .NET 11 package line. The user decided
  to close #4 and #5;
- its PRs target `main`, with titles like "Bump …".

The work goes on `chore/first-release` before its PR is opened, or on `dev` once it exists. Confirm with the
user first.

- [X] T030 In `.github/dependabot.yml`, for the `nuget` ecosystem:
      - add `target-branch: dev`, to take effect once `dev` exists;
      - add `commit-message: { prefix: "build", include: "scope" }`, so the titles become `build(deps): …`, which
        passes `pr-title` and releases nothing by itself;
      - add `ignore` rules with `update-types: ["version-update:semver-major"]` for `Microsoft.Extensions.*` and
        `System.Text.Json`, which the published packages depend on. Security updates are not affected by
        `ignore` on version updates. Confirm ⚠️ against the Dependabot options reference.

      For `github-actions`: add `target-branch: dev` and `commit-message: { prefix: "ci" }`.
- [X] T031 Create `.github/workflows/dependabot-lockfiles.yml` (option a):
      - trigger on `pull_request` (opened, synchronize, reopened), run only when
        `github.event.pull_request.user.login == 'dependabot[bot]'`, with `permissions: contents: read`;
      - check out the PR head with an App token (`actions/create-github-app-token`, the same App as release-please,
        with Contents write), so the push triggers CI again. `GITHUB_TOKEN` pushes do not start workflows;
      - pin the SDK from `global.json`, run `dotnet restore Viana.Results.slnx --force-evaluate`, and if any
        `packages.lock.json` changed, commit `build(deps): regenerate lock files` and push to the PR branch;
      - all actions pinned by SHA and at least 7 days old;
      - Dependabot PRs cannot read regular Actions secrets, so the App secrets must also be added as **Dependabot
        secrets**. Document this in `RELEASING.md`. Confirm ⚠️.
- [X] T032 In `RELEASING.md`, add the Dependabot section: the App secrets as Dependabot secrets, the
      lock-file workflow, the majors ignored for library-facing packages and why, and how to handle a Dependabot
      PR that still fails.
- [X] T033 Validate: YAML parses, every action is pinned by SHA, and the local build and tests are green. Tell the
      user how to re-run PR #2 after the workflow lands (comment `@dependabot recreate` or push), and how to close
      #4 and #5 with `@dependabot ignore this major version`.
      **Result (2026-11-26):** all actions are pinned by SHA, and the locked restore, the Release build (0
      warnings) and the tests pass. The YAML was not checked with a parser locally, because installing one was not
      allowed; GitHub validates the workflows on push. The Dependabot options (`ignore` with `update-types`,
      `target-branch`, `commit-message`) were checked against the official options reference. With `target-branch`,
      the options do not apply to security updates, which always target the default branch.

## Phase 11: one release PR and one publish run (2026-11-26)

The user found the flow too verbose: one release PR per package, and one `release.yml` run per tag (each run
restores, builds, tests and asks for its own approval). Decided:

- **one combined release PR** per cycle;
- **one publish run** per release, inside the release-please workflow, with one approval that publishes every
  released package.

The CI on PRs and merges stays as it is.

The work goes on `dev` (through a PR), before PR #9 (`dev` → `main`) is merged if possible, so the first release
already uses the new flow. Confirm the branch with the user first.

- [X] T034 In `release-please-config.json`, set `"separate-pull-requests": false`, so one release PR carries every
      package that changed, each with its own version and CHANGELOG. Confirm ⚠️ the combined PR title and the
      tags (still `<component>-v<version>`, one per package) against the release-please manifest docs.
- [X] T035 Move publishing into `.github/workflows/release-please.yml`.
      - **`release-please` job:** keep it, and expose the action outputs `releases_created` and `paths_released`
        (a JSON array of package paths). Confirm ⚠️ these names against googleapis/release-please-action v5,
        whose `action.yml` declares no outputs.
      - **`publish` job:**
        - runs if `releases_created == 'true'`, in `environment: nuget`, with `permissions: contents: write,
          id-token: write, attestations: write`;
        - checks out the release commit, with the SDK from `global.json`;
        - runs a locked restore, a Release build and the tests, once;
        - for each released path, maps the path to its component and project, finds its previous tag as the
          baseline, and runs `dotnet pack` with package validation (`-p:GeneratePackageOnBuild=false
          -p:VianaValidationProject=… -p:VianaValidationBaseline=…`);
        - generates one SBOM, then attests provenance and the SBOM for all `.nupkg` files;
        - logs in with `NuGet/login` right before pushing, pushes every package, and uploads each package to its
          own GitHub release.
      - All actions are pinned by SHA (the ones already used in `release.yml`).
- [X] T036 Delete `.github/workflows/release.yml`. Tags no longer start a workflow, and the ancestry check is not
      needed: release-please creates the tags on `main` itself.
- [X] T037 Update `RELEASING.md`:
      - the flow: one release PR, then one publish run with one approval;
      - **the nuget.org Trusted Publishing policy must name the workflow `release-please.yml`** (it named
        `release.yml`), with environment `nuget`. The user updates the policy before the first release;
      - the `nuget` environment's deployment refs: `main`, instead of the `*-v*` tags;
      - the required checks no longer mention `release.yml`.
- [X] T038 Validate: every action is pinned by SHA, the workflow logic has no references left to `release.yml`,
      and the local pack-with-validation commands match the new job. Tell the user the order:
      1. update the Trusted Publishing policy;
      2. merge this PR into `dev`;
      3. merge PR #9 `dev` → `main` (merge commit);
      4. merge #8;
      5. review the single release PR and merge it;
      6. approve the publish run once.
      **Result (2026-11-26):** `releases_created` and `paths_released` were confirmed in the
      release-please-action v5 README. A simulation of the publish mapping (path → component → version → project →
      baseline tag) is correct for all 6 packages, with AspNetCore having no baseline. 0 actions are pinned by tag,
      and no workflow references `release.yml` any more. The six release PRs (#10–#15) were closed by the user;
      they keep the `autorelease: pending` label, which the user should remove, so release-please does not treat
      them as pending releases.

## Baselines

Checked on nuget.org (flat-container index) on 2026-11-26. Every baseline exists:

- Viana.Results: 1.0.0, 1.0.1, 1.0.2, 2.0.0
- Viana.Results.Mvc: 1.0.0, 2.0.0
- Viana.Results.Mediators: 1.0.0
- Viana.Results.OpenApi: 1.0.0
- Viana.Results.OpenApi.Swashbuckle: 1.0.0, 1.0.1, 1.1.0
- Viana.Results.AspNetCore: not published, so it has no baseline (expected).

## Intended breaks

Real breaking changes of the public API against the released packages (package validation, 2026-11-26). Both
lists are recorded in `CompatibilitySuppressions.xml`, for the user to review.

- **Viana.Results.OpenApi vs 1.0.0** (8):
  - CP0001 for the 7 former helpers: `OpenApiDocumentProcessor`, `OpenApiOperationProcessor`,
    `OpenApiSchemaProcessor`, `ResolvedExample`, `ResponseExampleResolver`, `ProblemFieldSchema` and
    `ProblemResultSchema`;
  - CP0002 for `NoResponseBodyOperationTransformer()`, the parameterless constructor. The 2026-11-26 manual
    audit missed this one; the tool found it.
- **Viana.Results.OpenApi.Swashbuckle vs 1.1.0** (2): CP0002 for the constructors of `NoResponseBodyOperationFilter`
  and `ProblemResponseOperationFilter`. Both are binary breaks.
- **Viana.Results 2.0.0, Mvc 2.0.0 and Mediators 1.0.0**: no public API break. Their breaking changes are
  behavioral only (204, content type, telemetry tags...), which package validation cannot see.

Two findings while setting this up:

1. The baseline package is only downloaded at restore time, so `pack --no-build` after a plain restore fails.
   Pack restores and builds per project.
2. A global `-p:PackageValidationBaselineVersion` also applies to referenced projects (wrong or missing
   baselines). The baseline is now scoped to the validated project (`VianaValidationProject` /
   `VianaValidationBaseline` in `src/Directory.Build.props`). `ApiCompatPermitUnnecessarySuppressions` keeps
   normal builds green.

## Benchmark

After the AOT fixes (converter uses `JsonElement.ParseValue` and `options.GetTypeInfo`), ShortRun on net10.0, the
allocation is the same as in 015 for every case:

| Case | Allocated |
|---|---|
| MvcSuccess | 2.67 KB |
| AspNetCoreSuccess | 3.22 KB |
| MinimalSuccess | 3.00 KB |
| Failures | 2.55 / 3.73 / 3.72 KB |
| MvcPaged | 3.28 KB |
| AspNetCorePaged | 3.83 KB |
| MinimalPaged | 3.61 KB |

Mean times are within the ShortRun error margins.

## Guards

Local guards: each change was made on the working tree and reverted.

| Guard | Result |
|---|---|
| Locked restore | Changing `xunit` 2.9.3 → 2.9.0 in `Directory.Packages.props` fails with NU1004; after the revert it passes |
| Formatting | A scratch file with bad spacing fails `dotnet format --verify-no-changes` (exit 2); after the removal it passes |
| Removed public member | Without `CompatibilitySuppressions.xml`, pack of OpenApi vs 1.0.0 fails (CP0001/CP0002); with it, it passes |
| AOT | Reflection-based `JsonSerializer.Serialize/Deserialize` in the smoke app fails the build (IL2026/IL3050). The fixed app publishes for `win-x64` and runs (`AOT smoke: ok`) |
| Vulnerable dependency | NuGet audit (NU1903) blocked `Microsoft.DotNet.ILCompiler` 10.0.12 (GHSA-p3jp-9v83-76w9). It was fixed by pinning SDK 10.0.404 (runtime 10.0.15) |

Other checks: 0 actions are referenced by tag, and no workflow or doc names another mediator library.

Guards that need GitHub, to be validated by the user on a test PR or with an authorized push:

- the dependency review on a PR;
- the Dependabot cooldown and its CPM/lock-file support;
- the CodeQL run;
- the Linux and Windows matrix;
- the release dry run (release-please PR → tag → approval → Trusted Publishing → attestations), which first
  needs the one-time settings from `RELEASING.md`.

T019 was written and the actions' inputs were checked against their `action.yml`, but the workflow was not
executed.

## Dependencies

- T001 → T012/T013.
- Setup (T002, T003) → Foundational (T004, T005).
- US1, then US2: T010 → T011, T012 → T013, with T014 and T015 in parallel.
- US3 (T016 → T017 → T018 → T019).
- US4 → Polish.
- `ci.yml` is touched by T008, T012 and T014, so they run in that order.

## Parallel examples

- T002 ∥ T003.
- T006 ∥ T007.
- T014 ∥ T015.

## Implementation strategy

The MVP is US1, the supply chain. Then US2, the broken-code guards plus the AOT fixes, US3, publishing and
release-please, and US4, the checklist. Commit docs (specs, RELEASING.md, CHANGELOGs), code (src, tests) and
pipeline/config (`.github`, root config, lock files) separately. Formatting (T005) is its own commit.
