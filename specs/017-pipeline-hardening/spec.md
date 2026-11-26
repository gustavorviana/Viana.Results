# Feature Specification: Pipeline hardening (supply chain, broken code, safe publishing, automated versions)

**Feature Branch**: `chore/add-spec-kit`

**Created**: 2026-11-26

**Status**: Ready

**Input**: "My pipeline is weak: no dependency verification, nothing that catches broken code, only Dependabot.
Make it secure and reliable, with a minimum age for new dependency versions."

## Clarifications

### Session 2026-11-26

- Q: Lock the dependency graph? → A: Yes, with NuGet lock files (`packages.lock.json`, restored in locked mode in
  CI).
- Q: Who decides versions? → A: release-please, per package (path-based, Conventional Commits), keeping the
  `<component>-v<version>` tags. `<Version>` is never edited by hand.
- Q: Minimum age (cooldown) for new dependency versions? → A: 7 days. Security updates are not delayed.
- Q: Tags created with `GITHUB_TOKEN` do not trigger other workflows. How does the release tag start
  publishing? → A: release-please uses a GitHub App token (least privilege), so its tag triggers `release.yml`.
- Q: Which operating systems does CI cover? → A: Linux and Windows for build and tests. The AOT smoke app,
  CodeQL and the release run on Linux.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Dependencies can be trusted (Priority: P1)

As the maintainer, I want every build to use exactly the dependencies I reviewed, from the sources I allow. New
versions are only proposed after 7 days, and PRs that add a vulnerable or disallowed dependency are blocked.

**Why this priority**: supply-chain attacks (hijacked packages and actions, dependency confusion) are today's main
risk for a library.

**Independent Test**:

- a restore that would resolve any dependency differently from the lock files fails;
- a package from a source not mapped for it is not used;
- a PR adding a package with a known vulnerability fails the dependency review;
- Dependabot opens PRs for NuGet and GitHub Actions, never for versions younger than 7 days, except security
  updates.

**Acceptance Scenarios**:

1. **Given** the CI restore, **When** a transitive dependency would change version or content, **Then** the build
   fails until the lock files are updated in a reviewed PR.
2. **Given** a package that exists on another feed with the same name, **When** restoring, **Then** only the mapped
   source (nuget.org) is used.
3. **Given** a new release of a dependency, **When** fewer than 7 days have passed, **Then** Dependabot does not
   propose it, unless it fixes a security advisory.
4. **Given** a PR that adds or updates a dependency with a known vulnerability (or a disallowed license), **When**
   CI runs, **Then** the PR is blocked.
5. **Given** the workflows, **When** they reference an action, **Then** the reference is a full commit SHA, kept
   up to date by Dependabot.

---

### User Story 2 - Broken code does not merge (Priority: P1)

As the maintainer, I want CI to fail on code that breaks users:

- public API changes that are breaking compared with the published packages;
- code that is not formatted;
- security issues found by static analysis;
- loss of the core's trimming/AOT compatibility.

**Why this priority**: the 2026-11-26 audit found breaking API changes only by hand. CI must catch them.

**Independent Test**:

- removing a public member that exists in the published package fails CI, unless it is declared as an intended
  breaking change;
- an unformatted file fails CI;
- the static-analysis job runs on every PR;
- an AOT-published smoke app of the core builds and runs.

**Acceptance Scenarios**:

1. **Given** a package whose public API differs from its last published version in a breaking way, **When** CI
   builds, **Then** it fails and names the member, unless the break is recorded as intended for a major release.
2. **Given** an unformatted file, **When** CI runs, **Then** it fails.
3. **Given** a PR, **When** CI runs, **Then** static analysis runs and reports to the repository's security view.
4. **Given** the core, **When** CI runs, **Then** a smoke app is published with Native AOT and executed
   successfully (constitution VI).
5. **Given** CI, **When** it runs, **Then** it uses the SDK pinned by the repository, and builds and tests on
   Linux and Windows.

---

### User Story 3 - Publishing is safe and versions are automatic (Priority: P2)

As the maintainer, I want release-please to propose each package's next version from its own commits, and
publishing to need my approval. Publishing uses no long-lived secret, and the published packages carry
verifiable provenance.

**Why this priority**: a stolen API key or a tag pushed by mistake publishes to all users. Manual versions already
produced a wrong bump.

**Independent Test**:

- a `fix:` commit touching only `src/Viana.Results.Mediators/` makes release-please propose a Mediators patch and
  nothing else;
- merging the release PR tags `mediators-vX.Y.Z`;
- the publish job waits for approval, authenticates without a stored API key, and attaches provenance.

**Acceptance Scenarios**:

1. **Given** commits touching only one package folder, **When** release-please runs, **Then** only that package
   gets a release PR, with the bump from the commit types (`fix` → patch, `feat` → minor, `!`/`BREAKING CHANGE` →
   major).
2. **Given** a merged release PR, **When** the tag is created, **Then** it keeps the current format
   (`core-v*`, `mediators-v*`, `mvc-v*`, `openapi-v*`, `openapi-swashbuckle-v*`, plus `aspnetcore-v*`), and the
   csproj `<Version>` and CHANGELOG are updated by release-please.
3. **Given** a release tag, **When** the release workflow runs, **Then** publishing waits for manual approval in a
   protected environment, and it fails if the tagged commit is not on the release branch.
4. **Given** the publish step, **When** it pushes to NuGet, **Then** it authenticates with a short-lived token
   (Trusted Publishing), not a stored API key.
5. **Given** a published package, **When** a user inspects the GitHub release, **Then** it carries a build
   provenance attestation and an SBOM.

---

### User Story 4 - Repository settings back the pipeline (Priority: P3)

As the maintainer, I want a checklist of the GitHub settings that the files cannot enforce, so the pipeline cannot
be bypassed.

**Independent Test**: the checklist exists and each item says where to configure it.

**Acceptance Scenarios**:

1. **Given** the checklist, **When** it is followed, **Then** the release branch requires green CI and reviews,
   release tags are protected, secret scanning with push protection is on, and Actions use the minimum token
   permissions by default.

### Edge Cases

- Shared files (`Directory.Packages.props`, `Directory.Build.props`) belong to no package folder. A dependency
  update there must release the packages that use it. The plan decides how.
- A breaking change in the core that changes the visible behavior of a dependent package (for example Mvc) needs a
  commit that also touches the dependent package, or a manual release-please override. The plan documents this.
- Cross-platform lock files: lock files are generated for every target framework and must not differ between
  Windows and Linux.
- Security updates bypass the 7-day cooldown.
- The `<Version>` 2.0.0 set by hand in Viana.Results.OpenApi (commit 6abd45f) is reset to the last released value,
  so release-please proposes 2.0.0 from the `feat(openapi)!:` commit.
- The existing release notes in `<PackageReleaseNotes>` become the seed of each package's CHANGELOG.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Restores MUST use lock files, and CI MUST restore in locked mode.
- **FR-002**: A NuGet configuration MUST map every package to nuget.org (package source mapping).
- **FR-003**: Dependabot MUST be configured in the repository for NuGet and GitHub Actions, with a 7-day cooldown
  that does not apply to security updates.
- **FR-004**: PRs MUST run a dependency review that blocks known vulnerabilities and disallowed licenses.
- **FR-005**: Every action reference MUST be pinned to a full commit SHA. Workflows MUST declare least-privilege
  `permissions`.
- **FR-006**: The SDK MUST be pinned by a `global.json`.
- **FR-007**: CI MUST validate each package's public API against its last published version and fail on
  unrecorded breaking changes.
- **FR-008**: CI MUST check formatting and run static analysis (security queries) on PRs and on the release branch.
- **FR-009**: CI MUST publish and run a Native AOT smoke app of the core.
- **FR-010**: release-please MUST version each package independently by path, keep the current tag format,
  update `<Version>` and a per-package CHANGELOG, and open release PRs.
- **FR-011**: The release workflow MUST run in a protected environment with manual approval, verify that the tag
  is on the release branch, publish with NuGet Trusted Publishing, and attach a provenance attestation and an
  SBOM.
- **FR-012**: A settings checklist MUST describe the GitHub configuration that files cannot enforce.
- **FR-013**: The pipeline MUST NOT name any other mediator library.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 0 actions referenced by tag; 100% pinned by SHA.
- **SC-002**: A deliberate test change for each guard fails CI. The changes are an unlocked dependency, a
  vulnerable package, an unformatted file, a removed public member and an AOT warning in the core.
- **SC-003**: No dependency version younger than 7 days is proposed, except security fixes.
- **SC-004**: No long-lived NuGet secret is needed to publish.
- **SC-005**: A commit touching one package produces a release proposal for that package only.

## Assumptions

- The release branch is `main` today and may become the target of the planned dev-branch flow. The workflows take
  it as a parameter.
- Features of GitHub, Dependabot, NuGet and release-please, such as cooldown, Trusted Publishing, the generic XML
  updater and package validation, are confirmed against their current documentation during planning.
- Repository settings are applied by the user. The feature provides the checklist.
