# Releasing

This document describes how to publish a new version of any package in this
repository to [nuget.org](https://www.nuget.org).

Releases are driven by **git tags**. Pushing a tag with the right format
triggers the [`release.yml`](.github/workflows/release.yml) workflow, which
validates, builds, packs, publishes to NuGet, and creates a GitHub Release
with the `.nupkg` / `.snupkg` attached.

---

## One-time setup

1. Create a NuGet API key at <https://www.nuget.org/account/apikeys> with
   scope limited to the five package IDs:
   - `Viana.Results`
   - `Viana.Results.Mediators`
   - `Viana.Results.Mvc`
   - `Viana.Results.OpenApi`
   - `Viana.Results.OpenApi.Swashbuckle`
2. Add the key to the repo: **Settings → Secrets and variables → Actions →
   New repository secret** → name it `NUGET_API_KEY`.

That's it — the workflow uses `secrets.GITHUB_TOKEN` (provided automatically)
for the GitHub Release step.

---

## Tag convention

Tags follow the format `<slug>-v<semver>`:

| Tag prefix | Package |
|---|---|
| `core-v*` | `Viana.Results` |
| `mediators-v*` | `Viana.Results.Mediators` |
| `mvc-v*` | `Viana.Results.Mvc` |
| `openapi-v*` | `Viana.Results.OpenApi` |
| `openapi-swashbuckle-v*` | `Viana.Results.OpenApi.Swashbuckle` |

Examples: `core-v2.1.0`, `mediators-v1.0.0`, `openapi-swashbuckle-v1.2.3`.

---

## Releasing a package

1. **Bump `<Version>`** in the target `.csproj` (e.g.
   `src/Viana.Results/Viana.Results.csproj`).
2. **Update `<PackageReleaseNotes>`** in the same `.csproj` to describe the
   new version. These notes become both the NuGet release notes (shown on
   nuget.org) and the GitHub Release body.
3. Commit and push to `main`:
   ```bash
   git add src/Viana.Results/Viana.Results.csproj
   git commit -m "Viana.Results 2.1.0"
   git push
   ```
4. Wait for [CI](.github/workflows/ci.yml) to pass on `main`.
5. Create and push the tag:
   ```bash
   git tag core-v2.1.0
   git push origin core-v2.1.0
   ```
6. Watch the [Release workflow](../../actions/workflows/release.yml) run.
   When it finishes you'll see the package on nuget.org and a new GitHub
   Release in the repo.

To release multiple packages at once, push several tags in one shot — each
runs independently and in parallel:

```bash
git tag mvc-v2.1.0
git tag openapi-v1.1.0
git push --tags
```

---

## What the workflow validates

Before publishing anything, [`release.yml`](.github/workflows/release.yml)
runs these checks. Any failure aborts the release **before** anything is
pushed to NuGet:

1. **Tag format** — must match a known slug (`core-v*`, `mediators-v*`,
   `mvc-v*`, `openapi-v*`, `openapi-swashbuckle-v*`).
2. **Version match** — the version in the tag must equal `<Version>` in
   the target `.csproj` (read via `dotnet msbuild -getProperty:Version`).
3. **Release notes present** — `<PackageReleaseNotes>` must be non-empty.
4. **Tests pass** — `dotnet test` runs over the whole solution.

NuGet push uses `--skip-duplicate`, so re-running the workflow on the same
tag is safe and idempotent.

---

## Troubleshooting

**`Tag version (X.Y.Z) does not match csproj <Version> (A.B.C)`**

You bumped the tag but forgot to bump (or commit) the `.csproj`. Fix:

```bash
git tag -d <bad-tag>                 # delete the local tag
git push origin :refs/tags/<bad-tag> # delete the remote tag
# edit the csproj, commit, then re-tag with the right version
```

**`<PackageReleaseNotes> is empty`**

Add release notes to the `.csproj` and re-tag. Same cleanup as above.

**`Response status code does not indicate success: 403 (Forbidden)`**

`NUGET_API_KEY` is missing, expired, or doesn't have permission for that
package ID. Generate a new key (scoped to all five package IDs) and update
the secret.

**Tag pushed but nothing happened**

Check that the tag matches a configured prefix in
[`release.yml`](.github/workflows/release.yml) under `on.push.tags`. Tags
that don't match are silently ignored.

**Need to re-publish a tag**

Just re-run the failed workflow from the Actions UI. NuGet push is
idempotent (`--skip-duplicate`); the GitHub Release step will fail if a
release already exists for that tag — delete the existing release first if
you really need to recreate it.

---

## Versioning

This repo follows [Semantic Versioning](https://semver.org/). Each package
versions independently — bumping the core does not require bumping the
others, and vice versa.

- **MAJOR** — breaking API or wire-format change. Document it clearly in
  `<PackageReleaseNotes>` with a `BREAKING:` prefix.
- **MINOR** — backwards-compatible feature additions.
- **PATCH** — backwards-compatible bug fixes.
