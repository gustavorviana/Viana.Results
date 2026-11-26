# Changelog

Versions are managed by release-please from Conventional Commits. The entries below were written
before release-please was adopted (feature 017) and are not yet released.

## Unreleased (since Viana.Results 2.0.0)

- Major release. No public API removed; deprecations announced for 4.0.0. Behavior change: default title (feature 002).
- Changed: a ProblemResult without a title now defaults to the HTTP reason phrase of its status (e.g. 404 "Not Found") instead of "Error"; unknown statuses keep "Error". Applies to constructors, ProblemBuilder and JSON deserialization. Explicit titles and types are unchanged.
- Added: ProblemResult.WithDetail for the RFC 9457 "detail" member; ProblemBuilder.BuildResult() and BuildResult<T>() return a failed result holding the built problem.
- Deprecated: ProblemResult.WithDescription and ProblemBuilder.WithDescription ("description" is not an RFC 9457 member). Use WithDetail. Reading "description" from JSON keeps working.
- Fixed: a successful typed result with a null value now produces a null body (was an empty object "{}").
- Fixed: a failed result keeps its problem body whatever its status (a 204 no longer drops it).
- Changed: Result<T> whose value is a custom struct is unwrapped like a class; only simple values (numbers, booleans, text, dates, identifiers, enums) keep the wrapper.
- Changed: problem extension names are case-insensitive (ProblemResult, ProblemBuilder, JSON reading): the same name twice keeps one member with the last value and the first casing.
- Fixed: empty ProblemResult.Extensions is read-only, like non-empty ones.
- Changed: Results.Ok() without a value returns status 204 No Content (was 200), the HTTP status of a success without a body. Results.Ok(value) and Results.Ok(string) stay 200; a result built with an explicit status (new Result(200)) keeps it.
- Added: IEnvelopeResult<T> for results whose whole object is the body (for example a cursor list): every public property is written except Status, Problem and the IHasExtensions container, whose entries go to the root (declared members win). Added: ResultResponseBody.GetBody(result, JsonSerializerOptions), which shapes envelopes with the application options. Changed: IPagedResult<T> is an envelope, so the paged special case is gone; PagedResult output is unchanged.
- Fixed: envelope shaping is decided from the type contract, so conditionally written Status/Problem never reach the body. Obsolete messages name the replacement without a version.
