# Quickstart: validate feature 017

## Local

```bash
dotnet restore Viana.Results.slnx --locked-mode     # succeeds with the committed lock files
dotnet build -c Release                             # 0 warnings
dotnet test -c Release
dotnet format Viana.Results.slnx --verify-no-changes
dotnet pack -c Release -p:PackageValidationBaselineVersion=<last tag>   # per package; passes, or only CompatibilitySuppressions entries
dotnet publish tests/Viana.Results.AotSmoke -c Release -r linux-x64     # on Linux/WSL; then run the binary
```

## Guards (SC-002): each change must fail CI

Run each change on a scratch branch and revert it afterwards. The lock, formatting, removed-member and AOT guards
run locally. The vulnerable-package guard (dependency review) and the release dry run need GitHub: the user
runs them on a test PR, or authorizes the push.

| Change | Expected failure |
|---|---|
| Change a transitive version without updating the lock files | locked restore |
| Add a package with a known advisory in a PR | dependency review, and the NuGet audit |
| Unformat a file | `dotnet format --verify-no-changes` |
| Remove a public member that exists in the published baseline | package validation (CP0002) |
| Add reflection-only JSON to a code path used by the smoke app | AOT publish (IL2026/IL3050) |

## Release dry run

1. Push a `fix(mediators): …` commit that only touches `src/Viana.Results.Mediators/`. release-please opens a
   release PR for Mediators only.
2. Merge the release PR. The tag `mediators-vX.Y.Z` is created, and `release.yml` waits for approval in the
   `nuget` environment.
3. Do not approve the first time: check that the ancestry check and the packaging run, and that no API key is
   used.

## Settings

Follow the checklist in `RELEASING.md`, and confirm every item in the GitHub UI and on nuget.org.
