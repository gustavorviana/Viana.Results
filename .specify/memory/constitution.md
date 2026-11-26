<!--
Sync Impact Report
- Version change: (template) → 1.0.0
- Modified principles: all template placeholders replaced with Principles I–IX
- Added sections: Purpose, Technical Constraints, Development Workflow, Governance
- Removed sections: none
- Templates requiring updates: none (templates read the constitution at runtime)
- Follow-up TODOs: none
-->

# Viana.Results Constitution

## Purpose

Viana.Results is a public, general-purpose library. Its reason to exist is the
combination it delivers, which every feature MUST reinforce and never dilute:

1. A dependency-free, AOT-compatible core usable anywhere (`netstandard2.0`).
2. RFC 9457 Problem Details as a contract, not a detail.
3. Native integration with the host framework instead of competing with it.
4. Accurate OpenAPI documentation generated from result types.

Features that do not serve these goals, or that duplicate what the framework
already provides, MUST NOT be added.

## Core Principles

### I. Minimal, Portable Core

`Viana.Results` is the foundation and MUST run anywhere .NET runs (ASP.NET, workers,
Windows Services, console, libraries). The core MUST NOT depend on frameworks or
third-party packages. The only allowed external dependency is `System.Text.Json`.
All integrations (ASP.NET Core, Mediators, OpenAPI, Swashbuckle, alternative
serializers) MUST live in satellite packages named `Viana.Results.*`.

### II. Broad Compatibility

The core and every framework-independent satellite MUST target `netstandard2.0`.
Newer TFMs are added only to enable features through conditional compilation
(`#if`), never to replace `netstandard2.0`. Framework satellites (e.g.,
`Viana.Results.AspNetCore`) target only the framework's currently supported LTS
releases. Removing a TFM is a breaking change (see Principle VII).

### III. Test-First (NON-NEGOTIABLE)

TDD is mandatory: tests are written first, MUST fail, and only then is the
implementation written (Red-Green-Refactor). Every package in `src/` MUST have a
matching test project in `tests/` (xUnit). No change is merged without tests.

### IV. RFC 9457 Problem Details (NON-NEGOTIABLE)

Every error representation MUST comply with RFC 9457 (Problem Details for HTTP APIs),
including top-level (flattened) extension members and the
`application/problem+json` media type. This applies to every serializer and every
satellite package.

### V. Performance and Simplicity

The library MUST be as fast and as readable as possible, and simple to use.

- No unnecessary allocations or uncached reflection in hot paths.
- Changes to hot paths (Result creation, serialization, ASP.NET Core result
  conversion, mediator dispatch) MUST include BenchmarkDotNet results
  (before/after) in the PR. Any time or allocation regression MUST be justified,
  or the PR is rejected.
- Public APIs favor clarity over cleverness; complexity MUST be justified in the
  plan. Features without a concrete use case are not built (YAGNI).

### VI. Native AOT and Trimming (Core)

The core package MUST be trimming- and Native AOT-compatible (`IsAotCompatible=true`
for net8.0+). No reflection-based serialization or unannotated reflection is
allowed; serialization of unknown types MUST go through the caller's
`JsonSerializerOptions` type info resolver. Compatibility is verified by an
AOT-published smoke test in CI. Satellite packages are exempt but MUST annotate
reflection-dependent APIs with `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`.

### VII. Semantic Versioning and Breaking Changes

Each package is versioned independently following SemVer. Breaking changes are
evaluated case by case and MUST be explicitly discussed and approved before
implementation; the rationale MUST be recorded in the feature's spec/plan.
Approved breaking changes require a MAJOR version bump. Replaced packages are
deprecated on nuget.org pointing to their alternate package, never left silent.

### VIII. Documented Public Surface

Every public API MUST have XML documentation. Every package MUST have its own
README.md, updated with each feature that changes its behavior or API. Projects
in `examples/` MUST be updated whenever a feature changes the public API.
The repository README MUST state the library's positioning (see Purpose) and
keep an honest, up-to-date comparison with the main alternatives
(e.g., ErrorOr, Ardalis.Result, FluentResults, native `TypedResults`).

### IX. Native Framework Interoperability

Framework satellites MUST delegate to the host framework's native types and
services instead of reimplementing them, and MUST NOT write to the response stream
directly. In ASP.NET Core, Viana results MUST be converted to native results:

- MVC: `ObjectResult` with `ProblemDetails`/`ValidationProblemDetails` created via
  `ProblemDetailsFactory`, and `StatusCodeResult` for bodiless responses, so that
  content negotiation, output formatters (including `AddNewtonsoftJson`) and
  global problem details customization apply unchanged.
- Minimal APIs: native `TypedResults` (`Ok`, `Problem`, `ValidationProblem`,
  `NoContent`, etc.).

Mapping rules (status, problem type, unwrapping, extensions) MUST be implemented
once and shared by both adapters.

Precedence MUST be: values set on the Viana result → native defaults fill only
missing fields → application-level customization (`CustomizeProblemDetails`)
applies last. This precedence MUST be covered by tests on every supported TFM,
for both MVC and Minimal APIs.

## Technical Constraints

- **Language & build**: `LangVersion=latest`, `Nullable=enable`,
  `GenerateDocumentationFile=true`, `TreatWarningsAsErrors=true` in all packages.
- **Dependencies**: package versions are managed only in `Directory.Packages.props`
  (Central Package Management). New dependencies in satellites MUST be justified
  in the plan; new dependencies in the core are forbidden (Principle I).
- **Serialization**: `System.Text.Json` is the built-in serializer. In ASP.NET Core,
  serializer choice is delegated to the framework (Principle IX). Any other
  serializer support outside ASP.NET Core MUST live in a dedicated satellite
  package and MUST produce RFC 9457-compliant output.
- **Benchmarks**: live in `benchmarks/`, run locally in Release; they are not run in CI.

## Development Workflow

- **Branches & PRs**: `main` is protected. Every change goes through a Pull Request
  with review; direct pushes are not allowed.
- **Commits**: Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`, `test:`,
  `refactor:`, `perf:`; `!` or `BREAKING CHANGE:` for breaking changes).
  Versions and changelogs are derived automatically from commits.
- **Quality gates**: CI (restore, build, test on the solution, plus the AOT smoke
  test) MUST be green before merge.
- **Definition of done**: tests written first and passing; XML docs on public API;
  package README updated; examples updated if the API changed; benchmarks attached
  if a hot path changed; no new warnings.
- **Language**: specs, plans, docs, READMEs, XML docs and commit messages are
  written in English.

## Governance

This constitution supersedes all other practices. Every spec, plan, task list and
PR review MUST verify compliance with it; violations MUST be justified in the plan
or rejected. Amendments require a PR that updates this document, states the
rationale, and bumps its version (MAJOR: principle removed or redefined;
MINOR: principle or section added; PATCH: clarifications).

**Version**: 1.0.0 | **Ratified**: 2026-11-21 | **Last Amended**: 2026-11-21
