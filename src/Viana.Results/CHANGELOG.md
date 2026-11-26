# Changelog

Versions are managed by release-please from Conventional Commits.

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
