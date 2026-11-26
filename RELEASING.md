# Releasing

Versions are decided by [release-please](https://github.com/googleapis/release-please) from Conventional Commits.
Never edit `<Version>` by hand.

## Branches

```
feature/x ──squash──► dev ──merge commit──► main ──► release-please
```

- **Feature work → `dev`:** open a PR into `dev` and **squash** it. The PR title becomes the commit, so it must be a
  Conventional Commit (`feat(openapi): …`, `fix(core): …`, `feat!: …`). The `pr-title` check enforces this.
- **`dev` → `main`:** open a PR from `dev` into `main` and merge it with a **merge commit**, never squash. Every
  commit from `dev` then reaches `main` intact, and release-please reads each one. The title of this PR is free.
- One `dev` → `main` merge can carry several features into the same release.

## Flow

1. **Commit with Conventional Commits.** The package is decided by the folder the commit touches:

   | Commit | Bump |
   |---|---|
   | `fix: …` | patch |
   | `feat: …` | minor |
   | `feat!: …` or a `BREAKING CHANGE:` footer | major |

   A commit that touches `src/Viana.Results.Mediators/` only releases Mediators. Changes to root files
   (`Directory.*.props`, workflows) release nothing on their own.
2. **One release PR.** On every push to `main`, release-please opens or updates a single release PR that carries
   every package that changed, each with its new `<Version>` and `CHANGELOG.md` entry.
3. **Merge the release PR.** release-please creates one `<component>-v<version>` tag and GitHub release per
   package. The components are `core`, `mvc`, `mediators`, `openapi`, `openapi-swashbuckle` and `aspnetcore`.
4. **Publish, in one run.** The `publish` job of `release-please.yml` then runs once for the whole release:
   - waits for one approval in the `nuget` environment;
   - runs the locked restore, build and tests once;
   - packs each released package, with package validation against its previous release;
   - attests provenance and the SBOM;
   - pushes every package to NuGet with Trusted Publishing, and attaches them to their GitHub releases.

### Shipping a dependency-only fix

A dependency update in `Directory.Packages.props` releases nothing by itself. To ship it, add a commit that
touches the package and uses the `Release-As` footer, for example
`fix(mediators): ship the updated dependency` with `Release-As: 1.0.1`.

### Intended breaking changes

CI validates each package's public API against its latest version **published on nuget.org**, which is what users
have today (package validation, through `.github/scripts/nuget-baseline.sh`). A git tag whose package is not
published yet, for example after a failed publish, is not used. The publish job compares against the latest
published version lower than the one being released. For an intended break:

1. commit it as `feat!:`, with the reason;
2. record it by running
   `dotnet pack <project> -p:VianaValidationProject=<ProjectName> -p:VianaValidationBaseline=<last> -p:ApiCompatGenerateSuppressionFile=true`
   and committing the generated `CompatibilitySuppressions.xml`. The baseline is scoped to that project
   (`src/Directory.Build.props`), so referenced projects are not validated against the wrong version;
3. delete that file after the major is released.

### Verifying a published package

```bash
gh attestation verify Viana.Results.<x>.nupkg -R gustavorviana/Viana.Results
```

### If publishing fails after the release was created

release-please has already created the tags and GitHub releases, so a new push releases nothing. Re-running the
failed job reuses the old workflow code. To recover:

1. fix the cause and merge it into `main` (through `dev`);
2. in Actions → **Release Please** → **Run workflow**, choose `main` and give the JSON array of the package paths
   to publish (the default is all six);
3. approve the run in the `nuget` environment.

Versions come from `.release-please-manifest.json` on `main`, so run it before the next release PR is merged.
Packages that are already on NuGet are skipped (`--skip-duplicate`).

## Dependabot

- **Version updates** go to `dev`, at most weekly, 7 days after a version is published (cooldown). Their titles
  are `build(deps): …` (NuGet) and `ci(deps): …` (Actions). They pass `pr-title` and release nothing by
  themselves.
- **Security updates** are never delayed and always target the default branch.
- **Majors ignored**: `Microsoft.Extensions.*` and `System.Text.Json`. The published packages depend on them, and a
  library must depend on the lowest version that works. Raising them would force consumers (for example
  Mediators on net6/net8) onto a newer package line, so it is a deliberate change. To decline a proposed major,
  comment `@dependabot ignore this major version` on its PR.
- **Lock files**: `dependabot-lockfiles.yml` regenerates every `packages.lock.json` on Dependabot NuGet PRs and
  pushes the result, because Dependabot sometimes misses projects. It needs the App secrets as Dependabot secrets
  (settings step 4).
- **If a Dependabot PR still fails**:
  - for NU1004, run `dotnet restore Viana.Results.slnx --force-evaluate` locally and push the lock files;
  - for a real break, close the PR with `@dependabot ignore this version`, or fix the code in a PR to `dev`.

## One-time settings (follow in this order)

These settings live in GitHub and nuget.org, not in files.

1. **nuget.org Trusted Publishing.** On nuget.org, open the username menu, then Trusted Publishing. Add a policy with
   owner `gustavorviana`, repository `Viana.Results`, workflow **`release-please.yml`** and environment `nuget`.
   If a policy for `release.yml` exists (the earlier flow), change it to `release-please.yml`, or publishing
   fails to authenticate.
2. **Secret `NUGET_USER`.** Set it to your nuget.org profile name, not the email.
3. **Environment `nuget`.** In Settings → Environments, create it with yourself as a required reviewer, and allow
   only the `main` branch as deployment ref (publishing runs on the push to `main`, not on a tag).
4. **GitHub App for release-please.** Create a private App with these permissions: Contents read/write and Pull
   requests read/write. Install it on this repository only. Add the secrets `RELEASE_APP_CLIENT_ID` and
   `RELEASE_APP_PRIVATE_KEY` twice:
   - under Settings → Secrets and variables → **Actions**, for release-please;
   - under Settings → Secrets and variables → **Dependabot**, for the lock-file workflow on Dependabot PRs,
     which cannot read Actions secrets.
5. **Branches and rulesets.**
   - Create `dev` from `main`, and make `dev` the default branch, so new PRs target it.
   - In Settings → General → Pull Requests, allow both "squash merging" and "merge commits". The rulesets below
     decide which method each branch accepts.
   - `dev`:
     - require a PR, with "Allowed merge methods" set to **Squash** only;
     - require the checks `pr-title`, `build-and-test (ubuntu-latest)`, `build-and-test (windows-latest)`, `pack`,
       `format`, `aot-smoke`, `analyze` (CodeQL) and `dependency-review`;
     - block force pushes and deletion.
   - `main`:
     - require a PR, with "Allowed merge methods" set to **Merge** only, and PRs coming from `dev`;
     - require the same checks except `pr-title`;
     - block force pushes and deletion.
   - Tags `*-v*`: only the release-please App and admins can create them, and nobody can update or delete them.
6. **Security.**
   - Enable the **dependency graph** (Settings → Code security). The `dependency-review` workflow needs it and
     fails with `Forbidden` without it. On a private repository it also needs GitHub Advanced Security.
   - Enable secret scanning with push protection and Dependabot alerts.
   - In Settings → Actions, set the default workflow permissions to "Read repository contents".
7. **Publish one release with the new flow.**
8. **Only then revoke the old NuGet API key** on nuget.org, and delete the `NUGET_API_KEY` secret.

## First release (2026-11-26)

PR #1 reached `main` as a single squashed commit with a non-conventional title, so release-please found nothing
to release. It was unblocked once:

1. `release-as` was set temporarily in `release-please-config.json` (core 3.0.0, mvc 3.0.0, mediators 2.0.0,
   openapi 2.0.0, openapi-swashbuckle 2.0.0, aspnetcore 1.0.0);
2. a `feat!:` PR touching every package (its CHANGELOG) was merged into `main`.

Remove every `release-as` right after these releases are published, so later versions come from the commits again.
