# Implementation Plan: Pipeline hardening

**Branch**: `chore/add-spec-kit` | **Date**: 2026-11-26 | **Spec**: [spec.md](spec.md)

## Summary

The pipeline is hardened in four layers:

1. **Supply chain**: lock files, source mapping, Dependabot with a 7-day cooldown, dependency review, actions
   pinned by SHA, least-privilege tokens and a pinned SDK.
2. **Broken code**: public API validated against the published packages, formatting, CodeQL, and an AOT smoke app
   (which needs the core's 6 pre-existing AOT warnings fixed).
3. **Publishing**: release-please per package, an approved environment, Trusted Publishing and provenance plus
   SBOM.
4. **Settings checklist**: `RELEASING.md`.

Every csproj `<Version>` is reset to the last released tag, and release-please takes over versioning. See
[research.md](research.md) for each decision and the open defaults D1–D4.

## Technical Context

- **Language/Version**: GitHub Actions YAML, MSBuild, C# for the AOT smoke app and the AOT warning fixes.
- **Primary Dependencies**:
  - actions: `actions/checkout`, `actions/setup-dotnet`, `actions/dependency-review-action`,
    `github/codeql-action`, `googleapis/release-please-action@v4`, `NuGet/login@v1`,
    `actions/attest-build-provenance`, `actions/attest-sbom`, plus an SBOM generator;
  - the `dotnet format` tool.
- **Testing**: each guard is proven by a deliberate failing change in a scratch branch or run (SC-002). The full
  test suite stays green.
- **Constraints**: no long-lived NuGet secret. The workflow name `release.yml` is fixed by the Trusted Publishing
  policy. The release branch is a workflow input or variable (`main` now, `dev` flow later).
- **Scale/Scope**: about 5 workflow and config files, 1 new test project, edits to 6 csproj files and 2 props files,
  6 CHANGELOG seeds, `RELEASING.md`, and the core AOT fixes.

## Constitution Check

| Principle | Status |
|---|---|
| I–II | ✅ No runtime dependency added to the packages |
| III Test-first | ✅ Each guard is verified by a deliberate failing case, and the AOT fixes keep the suite green |
| V Performance | ✅ The AOT fixes touch the converter and reflection paths, so the benchmarks are rerun |
| VI AOT (core) | ✅ This feature adds the CI AOT smoke test the constitution requires (it closes a gap) |
| VII Breaking | ✅ Package validation enforces it in CI. Intended breaks are recorded in CompatibilitySuppressions.xml |
| VIII Docs | ✅ RELEASING.md, plus a CHANGELOG per package. READMEs stay minimal |

## Project Structure

```text
global.json                          # SDK pin (R6)
nuget.config                         # source mapping (R2)
Directory.Build.props                # lock files, package validation switch (R1, R7)
release-please-config.json           # per-package releases (R11)
.release-please-manifest.json        # seeded from the released tags
RELEASING.md                         # flow + settings checklist (R13)
.github/dependabot.yml               # nuget + github-actions, cooldown 7 (R3)
.github/workflows/ci.yml             # locked restore, build, tests, format, pack + API validation, AOT smoke (R1,R5–R8,R10)
.github/workflows/dependency-review.yml  # PR-only (R4)
.github/workflows/codeql.yml         # csharp, manual build (R9)
.github/workflows/release-please.yml # release PRs + tags (R11)
.github/workflows/release.yml        # publish on tag: ancestry check, approval, Trusted Publishing, attestations (R12)
src/*/CHANGELOG.md                   # seeded from <PackageReleaseNotes> (D4)
src/*/*.csproj                       # <Version> reset; PackageReleaseNotes → CHANGELOG link
src/Viana.Results/ProblemResultJsonConverter.cs, ResultReflections.cs  # AOT warning fixes (D1)
tests/Viana.Results.AotSmoke/        # PublishAot console app (R10)
packages.lock.json (per project)     # generated
```

## Complexity Tracking

| Item | Why | Simpler alternative rejected |
|---|---|---|
| Baseline computed from tags in CI | Avoids editing a baseline after every release | A fixed `PackageValidationBaselineVersion` in the csproj, which goes stale |
| CHANGELOG replaces PackageReleaseNotes | release-please owns the notes, so there is one source | Keeping both, which drift apart |
