# Quickstart: Validate Warnings as Errors

**Prerequisites**: .NET SDK 10.0.x, repository root as working directory.

## 1. Clean build has zero warnings (SC-001, SC-004)

```bash
dotnet restore Viana.Results.slnx --force
dotnet build Viana.Results.slnx -c Release --no-restore --no-incremental
```

Expected: `Build succeeded`, `0 Warning(s)`, `0 Error(s)`.

```bash
dotnet list Viana.Results.slnx package --vulnerable --include-transitive
```

Expected: no vulnerable packages reported.

## 2. A warning breaks the build (FR-001, FR-002)

Temporarily add a public, undocumented member to any `src/` package, e.g.
`public static int Probe;` in `src/Viana.Results/Results.cs`, then build.

Expected: build fails with `error CS1591`. Revert the change.

Repeat with a nullable warning in a test project (e.g. `string s = null;`).

Expected: build fails with `error CS8600`. Revert.

## 3. Example exemption is scoped (FR-005)

Add an undocumented public member to `examples/Viana.Results.Examples.Shared`.

Expected: build succeeds (CS1591 exempt in examples). Add `string s = null;` there instead.

Expected: build fails (`error CS8600`). Revert.

## 4. New projects inherit the rule (SC-002)

```bash
dotnet new classlib -o tests/Probe && echo 'public class C { public void M() { string s = null; } }' > tests/Probe/C.cs
dotnet build tests/Probe
```

Expected: build fails with `error CS8600`, with no setup in `Probe.csproj`. Delete `tests/Probe`.

## 5. Converter defaults (R5)

```bash
dotnet test tests/Viana.Results.Tests -c Release --filter "FullyQualifiedName~ProblemResultJsonConverter"
```

Expected: tests for a problem JSON without `title`/`type` pass, yielding `type = "about:blank"`
and `title` = status reason phrase.

## 6. CI (FR-006)

Push a commit containing a deliberate warning to a PR. Expected: the `CI` workflow fails at
restore or build. Remove it; CI passes.
