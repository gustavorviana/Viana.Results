# Research: Pipeline hardening

Each decision says whether its source was checked against the live documentation on 2026-11-26 (✅) or comes from
memory and must be confirmed during implementation (⚠️). Implementation tasks re-check every ⚠️ item before relying
on it.

## R1: Lock files ⚠️

- **Decision**: put `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>` in `Directory.Build.props`.
  CI restores with `dotnet restore --locked-mode`. The AOT smoke app (R10) sets it to false, because a RID-specific
  publish adds runtime and ILCompiler packs, and those differ between machines.
- **Rationale**: library and test projects have no RID, so their lock files are the same on Windows and Linux. A
  multi-target project keeps one section per TFM in the same file.
- **Gotchas**: an SDK bump can change implicit or analyzer packages, so the lock files are regenerated in the same
  PR as the `global.json` change. `CentralPackageTransitivePinningEnabled` is a separate opt-in, and it is not
  enabled.
- **Source**: https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files#locking-dependencies

## R2: Package source mapping ⚠️

- **Decision**: add a root `nuget.config` that clears all sources, adds nuget.org, and maps `*` to nuget.org.
- **Source**: https://learn.microsoft.com/nuget/consume-packages/package-source-mapping

## R3: Dependabot ✅ (cooldown) / ⚠️ (lock files, CPM)

- **Decision**: add `.github/dependabot.yml` with two ecosystems:
  - `nuget` (directory `/`): `cooldown: default-days: 7`;
  - `github-actions`: `cooldown: default-days: 7`. This ecosystem only supports `default-days`.

  Both run weekly, with grouped minor and patch updates.
- **Security updates**: the cooldown never applies to them (checked).
- **To confirm**: Dependabot NuGet updates `Directory.Packages.props` (Central Package Management) and refreshes
  `packages.lock.json` in the same PR.
- **Sources**: https://docs.github.com/en/code-security/dependabot/working-with-dependabot/dependabot-options-reference ,
  https://docs.github.com/en/code-security/dependabot/ecosystems-supported-by-dependabot/supported-ecosystems-and-repositories

## R4: Dependency review ⚠️

- **Decision**: a `dependency-review` job on `pull_request` with `fail-on-severity: moderate`,
  `allow-licenses` set to a permissive list (D2), and `comment-summary-in-pr: on-failure`.
- **Requirements**: the dependency graph must be on, which is free for public repositories.
- **Source**: https://github.com/actions/dependency-review-action

## R5: Actions pinned by SHA, least privilege

- **Decision**:
  - every `uses:` points to a full commit SHA with a `# vX.Y.Z` comment, kept current by Dependabot
    (`github-actions`);
  - every workflow starts with `permissions: contents: read`, and each job raises only what it needs.

## R6: SDK pin ⚠️

- **Decision**: add a `global.json` pinning the 10.0 SDK in use, with `rollForward: latestPatch`. CI uses
  `actions/setup-dotnet` with `global-json-file`. The net8.0 runtime is installed for the net8 test runs.
- **Operating systems**: build and tests run in a matrix of `ubuntu-latest` and `windows-latest`. The AOT smoke
  app, CodeQL and the release run on `ubuntu-latest`. Lock files are generated without a RID, so they are the
  same on both systems (R1).

## R7: Public API validation against the published packages ⚠️

- **Decision**:
  - set `EnablePackageValidation=true` in the packable projects;
  - CI computes the baseline per package from the latest `<component>-v*` git tag and passes it as
    `-p:PackageValidationBaselineVersion=<x>` to `dotnet pack`. This avoids hand-editing a baseline after every
    release;
  - a package with no tag yet, such as `Viana.Results.AspNetCore`, skips the baseline.
- **Intended breaks**: generated with `-p:ApiCompatGenerateSuppressionFile=true` and committed as
  `CompatibilitySuppressions.xml` next to the project. The file is the reviewed record of a major's breaking API,
  and is removed after that major ships.
- **Prerequisite**: the baseline versions must exist on nuget.org. The user said everything on `main` counts as
  published. The first implementation task confirms that each tagged version is really on nuget.org, and reports
  any gap instead of assuming.
- **Source**: https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/overview

## R8: Formatting

- **Decision**: CI runs `dotnet format Viana.Results.slnx --verify-no-changes`. A first task formats the
  repository once, as its own commit.

## R9: Static analysis ⚠️

- **Decision**: a CodeQL workflow for `csharp` with `build-mode: manual`. It uses the pinned SDK plus
  `dotnet build`, because this is the most accurate mode for multi-target .NET. It runs on PRs, on the release
  branch and weekly.
- **Source**: https://docs.github.com/en/code-security/code-scanning/creating-an-advanced-setup-for-code-scanning/codeql-code-scanning-for-compiled-languages

## R10: Native AOT smoke test ⚠️

- **Decision**: add `tests/Viana.Results.AotSmoke`, a console app on net10.0 with `PublishAot=true` and warnings
  as errors, that exercises results, problems and envelope bodies through `JsonTypeInfo`. CI publishes it with
  `-r linux-x64` and runs the binary. The core gets
  `IsAotCompatible` for TFMs compatible with net8.0.
- **Consequence**: the 6 pre-existing trim/AOT warnings in the core would fail the build. They are:
  - in `ProblemResultJsonConverter`, IL2026/IL3050 at lines 126 and 158;
  - in `ResultReflections`, IL2070 in `IsUnwrappableType` and `GetDataType`.

  They have to be fixed, with `JsonTypeInfo`-based calls and proper annotations or narrow justified
  suppressions, in this feature (D1).

## R11: release-please ✅ (tags, xml updater, paths) / ⚠️ (action outputs)

- **Decision**:
  - `release-please-config.json` has one package per `src/<Project>` folder, with `release-type: simple` and
    `component` set to `core`, `mediators`, `mvc`, `openapi`, `openapi-swashbuckle` or `aspnetcore`;
  - an `extra-files` entry of type `xml` updates `<Version>`, with xpath `//Project/PropertyGroup/Version` and a
    path relative to the package;
  - the default tag is `<component>-v<version>`, which keeps the current format;
  - the action is `googleapis/release-please-action@v4`.
- **Manifest seed** (`.release-please-manifest.json`): the last released tags, which are core 2.0.0, mvc 2.0.0,
  mediators 1.0.0, openapi 1.0.0 and openapi-swashbuckle 1.1.0. AspNetCore has no release, so its first one uses
  `release-as: 1.0.0`.
- **Versions reset**: every csproj `<Version>` is reset to the manifest value, because 3.0.0, 2.1.0, 1.1.0, 2.0.0
  and 1.1.2 were set by hand. release-please then proposes the bumps from the `feat!`/`feat`/`fix` commits since
  each tag.
- **Root-only changes** (`Directory.*.props`, workflows) trigger no release (checked). A dependency fix that must
  ship is released with a `Release-As` footer or an explicit `fix(<pkg>)` commit touching the package (D3).
- **Release notes** (D4): release-please writes `src/<Project>/CHANGELOG.md` and creates the GitHub release. It
  does not update `<PackageReleaseNotes>`. The existing notes are moved into each CHANGELOG as its seed, and
  `<PackageReleaseNotes>` becomes a link to that CHANGELOG.
- **Source**: https://github.com/googleapis/release-please/blob/main/docs/manifest-releaser.md ,
  https://github.com/googleapis/release-please/blob/main/docs/customizing.md

## R12: Publishing ✅ (Trusted Publishing) / ⚠️ (attestations)

- **Token**: GitHub ignores events created with `GITHUB_TOKEN` (they start no new workflow run), so the
  release-please workflow uses a GitHub App installation token (`actions/create-github-app-token`, with the app
  limited to this repository, contents and pull-requests write). Its tags then trigger `release.yml`. The App
  id and private key are stored as secrets (`RELEASE_APP_ID`, `RELEASE_APP_PRIVATE_KEY`). ⚠️ Confirm the action
  inputs.
- **Decision**: release-please creates the tag and the GitHub release. `release.yml` runs on that tag (the file
  name is fixed, because the Trusted Publishing policy names it). It then does the following:
  1. checks that the tagged commit is on the release branch (`git merge-base --is-ancestor`);
  2. runs build, tests and package validation;
  3. waits in the protected environment `nuget`, which requires approval;
  4. logs in with `NuGet/login@v1` (`id-token: write`, the nuget.org user from a secret) and pushes with the
     short-lived key;
  5. attests provenance with `actions/attest-build-provenance` and an SPDX SBOM with `actions/attest-sbom`;
  6. uploads the `.nupkg`, `.snupkg` and SBOM to the release.
- **Gotchas**: the temporary key lasts 1 hour, so log in right before pushing. Renaming `release.yml` breaks the
  policy. Attestations live on GitHub and are verified with `gh attestation verify`.
- **Source**: https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing

## R13: Settings checklist

- **Decision**: a root `RELEASING.md` holds two things:
  - **the release flow**: commits, the release PR, approval and publishing;
  - **the settings checklist**:
    - rulesets on the release branch: required checks, reviews and no force push;
    - a tag ruleset for `*-v*`;
    - secret scanning with push protection;
    - default `GITHUB_TOKEN` permissions set to read;
    - the `nuget` environment with a required reviewer;
    - the nuget.org Trusted Publishing policy;
    - revoking the old API key.

## Decisions for the user

The plan proceeds with these defaults unless the user changes them:

- **D1** Fix the 6 pre-existing AOT warnings in this feature, which is required for the smoke test to pass.
- **D2** License allow-list: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC and 0BSD.
- **D3** Dependency-only updates do not release automatically. Shipping one is an explicit decision (`Release-As`).
- **D4** CHANGELOG.md per package becomes the source of release notes, and `<PackageReleaseNotes>` links to it.
