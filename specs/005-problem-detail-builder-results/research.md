# Research: RFC 9457 "detail" and Builder Results

## R1 – Which public APIs are "description" APIs
- **Decision**: Deprecate `ProblemResult.WithDescription(...)` and `ProblemBuilder.WithDescription(...)`.
- **Not deprecated**: `ResponseExampleAttribute.Description`, `ProblemFieldSchema` / `ProblemResultSchema`
  `description` parameters, `VianaResultOptions` `description` (OpenAPI descriptions, a different concept).
- **`ProblemResultAttribute.Description`** (OpenApi): documented as the problem `description` extension
  but ignored by `ProblemResultSchema.FromAttribute` (only Status/Type/Title are used). In scope (US3):
  add `Detail`, wire it, deprecate `Description` (see R7).
- **Source**: `git grep WithDescription` and `Description` in `src/`.

## R2 – Obsolete message
- **Decision**: `"description" is not an RFC 9457 member and will be removed in Viana.Results 4.0.0. Use {replacement} (RFC 9457 "detail") instead.`
  with `{replacement}` = `ProblemResult.WithDetail` / `ProblemBuilder.WithDetail`.
- **Rationale**: States why, what to use and when it goes away (FR-002).
- RFC references: §3.1.4 "detail" (https://www.rfc-editor.org/rfc/rfc9457.html#section-3.1.4),
  §3.2 extension members.

## R3 – Avoid internal obsolete calls (TreatWarningsAsErrors)
- **Decision**: Add `internal static ProblemResult CreateWithDescription(...)` holding the current
  logic; `WithDescription` delegates to it; `ProblemBuilder.Build()` and the JSON converter call it.

## R4 – `WithDetail` storage
- **Decision**: Store under `Extensions["detail"]` (same key `ProblemBuilder.WithDetail` already uses);
  null/empty detail not stored; other extensions preserved; an explicit `extensions["detail"]` is
  overwritten by the `detail` argument when it is non-empty.

## R5 – Builder results
- **Decision**: `public Result BuildResult() => new Result(Build());` and
  `public Result<T> BuildResult<T>() => new Result<T>(Build());` using the existing public
  `Result(ProblemResult)` / `Result<T>(ProblemResult)` constructors (status taken from the problem).
- **Alternatives**: implicit conversion from builder to result (less discoverable, ambiguous with
  generic targets); list/paged variants (declined by owner).

## R6 – Versioning
- **Decision**: `Viana.Results` 3.0.0 (owner decision), release notes cover feature 002 (title default)
  and 005 (WithDetail, BuildResult, deprecations → removal in 4.0.0). Mediators and Mvc keep their
  versions (no code change); OpenApi and Swashbuckle are bumped by US3 (see R7).
- **Republishing satellites**: satellites reference the core via `ProjectReference`, so a re-pack would
  change their NuGet dependency to `Viana.Results >= 3.0.0`. A satellite is only re-packed with a version
  bump (PATCH at least); until then, the published satellite versions keep working with 3.0.0 (no API
  removed). This is a release-process rule (RELEASING.md), not a task of this feature.

## R7 – OpenAPI annotation `Detail`
- **Decision**: Add `public string? Detail { get; set; }` to `ProblemResultAttribute`. In
  `ProblemResultSchema.FromAttribute`, when `Detail` is not null/empty, call
  `AddField(JsonSchemaType.String, "detail", "RFC 9457: human-readable explanation specific to this occurrence", attribute.Detail)`.
  Swashbuckle reuses `OpenApiOperationProcessor.ApplyProblemResultAttributes`, so both pipelines get it.
- **Obsolete message**: `Description has no effect and will be removed in Viana.Results.OpenApi 2.0.0. Use Detail (RFC 9457 "detail") instead.`
- **Versions**: Viana.Results.OpenApi 1.0.1 → 1.1.0 (MINOR: additive + deprecation);
  Viana.Results.OpenApi.Swashbuckle 1.1.1 → 1.1.2 (PATCH: no own API change, new behavior via dependency).
- **Removal version**: next major of the OpenAPI package (2.0.0).
