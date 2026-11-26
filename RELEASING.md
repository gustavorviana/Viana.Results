# Releasing

Versions are decided by [release-please](https://github.com/googleapis/release-please) from Conventional Commits.
Never edit `<Version>` by hand.

## Flow

1. **Commit with Conventional Commits.** The package is decided by the folder the commit touches:

   | Commit | Bump |
   |---|---|
   | `fix: …` | patch |
   | `feat: …` | minor |
   | `feat!: …` or a `BREAKING CHANGE:` footer | major |

   A commit that touches `src/Viana.Results.Mediators/` only releases Mediators. Changes to root files
   (`Directory.*.props`, workflows) release nothing on their own.
2. **Release PRs.** On every push to `main`, release-please opens or updates one release PR per package. The PR
   carries the new `<Version>`, the `CHANGELOG.md` entry and the GitHub release notes.
3. **Merge a release PR.** release-please creates the tag `<component>-v<version>` and the GitHub release. The
   components are `core`, `mvc`, `mediators`, `openapi`, `openapi-swashbuckle` and `aspnetcore`.
4. **Publish.** The tag starts `release.yml`, which:
   - checks that the tag is on `main`, and that it matches `<Version>`;
   - runs build, tests and package validation;
   - waits for approval in the `nuget` environment;
   - attests provenance and the SBOM;
   - pushes to NuGet with Trusted Publishing.

### Shipping a dependency-only fix

A dependency update in `Directory.Packages.props` releases nothing by itself. To ship it, add a commit that
touches the package and uses the `Release-As` footer, for example
`fix(mediators): ship the updated dependency` with `Release-As: 1.0.1`.

### Intended breaking changes

CI validates each package's public API against its last released version (package validation). For an intended
break:

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

## One-time settings (follow in this order)

These settings live in GitHub and nuget.org, not in files.

1. **nuget.org Trusted Publishing.** On nuget.org, open the username menu, then Trusted Publishing. Add a policy with
   owner `gustavorviana`, repository `Viana.Results`, workflow `release.yml` and environment `nuget`.
2. **Secret `NUGET_USER`.** Set it to your nuget.org profile name, not the email.
3. **Environment `nuget`.** In Settings → Environments, create it with yourself as a required reviewer, and allow
   only the `*-v*` tags as deployment refs.
4. **GitHub App for release-please.** Create a private App with these permissions: Contents read/write and Pull
   requests read/write. Install it on this repository only. Add the secrets `RELEASE_APP_CLIENT_ID` and
   `RELEASE_APP_PRIVATE_KEY`.
5. **Rulesets.**
   - `main`: require a PR, require the checks `build-and-test (ubuntu-latest)`, `build-and-test (windows-latest)`,
     `pack`, `format`, `aot-smoke`, `analyze` (CodeQL) and `dependency-review`, and block force pushes and
     deletion. Add `dev` the same way when the dev flow starts.
   - Tags `*-v*`: only the release-please App and admins can create them, and nobody can update or delete them.
6. **Security.**
   - Enable secret scanning with push protection, the dependency graph and Dependabot alerts.
   - In Settings → Actions, set the default workflow permissions to "Read repository contents".
7. **Publish one release with the new flow.**
8. **Only then revoke the old NuGet API key** on nuget.org, and delete the `NUGET_API_KEY` secret.
