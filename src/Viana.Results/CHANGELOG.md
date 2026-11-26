# Changelog

Versions are managed by release-please from Conventional Commits.

## [3.0.0](https://github.com/gustavorviana/Viana.Results/compare/core-v2.0.0...core-v3.0.0) (2026-11-26)


### ⚠ BREAKING CHANGES

* see each package CHANGELOG.md (Results.Ok() answers 204, problem content type, telemetry tag names, internal OpenApi helpers, OpenAPI document shape).

### Features

* first release with release-please ([#7](https://github.com/gustavorviana/Viana.Results/issues/7)) ([b2ec70b](https://github.com/gustavorviana/Viana.Results/commit/b2ec70b3c3e15abee7f8eeb91b9471905ae86928))

## 3.0.0 (upcoming) (since 2.0.0)

### Breaking changes

- `Results.Ok()` without a value answers 204 No Content (was 200). `new Result(status)` keeps its status.
- A problem without a title defaults to the HTTP reason phrase of its status (was "Error").
- A successful typed result with a null value is written as `null` (was `{}`). A failure with status 204 keeps its problem body.
- `Result<T>` with a custom struct value is unwrapped like a class.
- Problem extension names are case-insensitive.
- `ProblemResult.WithDescription` and `ProblemBuilder.WithDescription` are obsolete. Use `WithDetail`.

### Added and fixed

- Added `IEnvelopeResult<T>`: the whole object is the response body, for list results with metadata such as a cursor. `PagedResult<T>` is an envelope, and its output is unchanged.
- Added `ProblemBuilder.BuildResult()` / `BuildResult<T>()`, `ProblemResult.WithDetail` (RFC 9457 `detail`) and `ResultResponseBody.GetBody(result, JsonSerializerOptions)`.
- Native AOT and trimming compatible on net8.0+ (verified by a PublishAot smoke test).
