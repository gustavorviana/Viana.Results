# Implementation Plan: ASP.NET Core Package with Native ProblemDetails Interop (MVC)

**Branch**: `chore/add-spec-kit` (shared) | **Date**: 2026-11-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/006-aspnetcore-mvc-interop/spec.md`

## Summary

New package `src/Viana.Results.AspNetCore` (net8.0; net10.0, `FrameworkReference Microsoft.AspNetCore.App`).
A `VianaActionResult` converts any Viana `IResult`:

- **Failure**: maps the Viana `ProblemResult` to a native `ProblemDetails` (Viana values set first),
  then writes it through the framework's `IProblemDetailsService`, so native defaults only fill what is
  missing and `CustomizeProblemDetails` runs last. Fallback when the service is not registered: native
  `ProblemDetailsFactory` + `ObjectResult` with `application/problem+json`.
- **Success**: `ObjectResult` with the body from `ResultResponseBody.GetBody` and the result status, so
  MVC content negotiation and formatters (System.Text.Json, Newtonsoft, XML) apply.

Public surface: `ToActionResult()` extensions and `AddVianaResultFilter()` (same name as the old
package). Tests compare Viana responses with native `Problem(...)` responses in an in-memory host.

## Technical Context

**Language/Version**: C# latest, .NET SDK 10; targets net8.0, net10.0

**Primary Dependencies**: `Microsoft.AspNetCore.App` (framework reference), `Viana.Results` (project reference)

**Storage**: N/A

**Testing**: xUnit + `Microsoft.AspNetCore.Mvc.Testing` (in-memory host) in
`tests/Viana.Results.AspNetCore.Tests` (net8.0; net10.0); Newtonsoft tests use
`Microsoft.AspNetCore.Mvc.NewtonsoftJson` (test-only); XML via built-in formatters

**Target Platform**: ASP.NET Core 8 and 10 apps

**Project Type**: library (NuGet), new package 1.0.0

**Performance Goals**: no unjustified allocation regression versus `Viana.Results.Mvc` (benchmarked)

**Constraints**: TreatWarningsAsErrors; no runtime dependency beyond the shared framework; old Mvc
package untouched (FR-011)

**Scale/Scope**: ~5 source files, ~40 tests, README, example update, 1 benchmark class

## Constitution Check

| Principle | Status | Note |
|-----------|--------|------|
| I. Minimal core | PASS | Core untouched; ASP.NET only in the satellite |
| II. TFMs | PASS | Framework satellite targets LTS only (net8.0, net10.0) |
| III. TDD | PASS | Parity/precedence tests written first |
| IV. RFC 9457 | PASS | Native ProblemDetails is RFC 9457; parity test enforces it |
| V. Performance | PASS | Conversion benchmark added (hot path "ASP.NET Core result conversion") |
| VII. SemVer | PASS | New package 1.0.0; old package unchanged |
| VIII. Docs | PASS | XML docs, package README, example app, root README table |
| IX. Native interop | PASS | Uses IProblemDetailsService / ProblemDetailsFactory; precedence Viana → native → Customize |

Post-design re-check: PASS.

## Project Structure

### Documentation (this feature)

```text
specs/006-aspnetcore-mvc-interop/
├── plan.md, research.md, data-model.md, quickstart.md
├── contracts/public-api.md, contracts/problem-mapping.md
└── tasks.md (next)
```

### Source Code

```text
src/Viana.Results.AspNetCore/
├── Viana.Results.AspNetCore.csproj
├── README.md
├── VianaActionResult.cs                 # IActionResult: success → ObjectResult; failure → problem pipeline
├── ProblemDetailsMapper.cs              # internal: ProblemResult → ProblemDetails (Viana values)
├── ResultActionResultExtensions.cs      # ToActionResult()
└── Filters/
    ├── VianaResultFilter.cs             # IResultFilter: ObjectResult{Value: IResult} → VianaActionResult
    └── VianaResultFilterExtensions.cs   # AddVianaResultFilter(this IMvcBuilder)
tests/Viana.Results.AspNetCore.Tests/
├── ProblemParityTests.cs                # Viana vs native Problem(...) for 400/401/403/404/409/422/500
├── ProblemPrecedenceTests.cs            # explicit values, extensions vs traceId, Customize last and once
├── SuccessFormattingTests.cs            # STJ, Newtonsoft, XML, 204, list unwrapped, paged wrapped
├── FilterTests.cs                       # direct return sync/async; non-result untouched
└── TestHost/                            # minimal controllers + host setups
benchmarks/Viana.Results.Benchmarks/AspNetCoreBenchmarks.cs
examples/                                # shared example switches from Viana.Results.Mvc to the new package
```

**Structure Decision**: New satellite mirroring the `Viana.Results.Mvc` layout, with its own test project.

## Complexity Tracking

None.
