# Changelog

Versions are managed by release-please from Conventional Commits. The entries below were written
before release-please was adopted (feature 017) and are not yet released.

## Unreleased (since Viana.Results.Mvc 2.0.0)

- Fixed: problem responses use the RFC 9457 media type application/problem+json (was application/json).
- Fixed: a successful typed result with a null value is written as JSON null (was "{}"); a failure with status 204 keeps its problem body (via Viana.Results 3.0.0).
- Improved: bodies are streamed to the response (no intermediate string) and writing stops when the request is aborted. A subclass overriding SerializeToJson keeps working.
- Fixed: calling AddVianaResultFilter more than once registers the filter once.
- Results.Ok() without a value now answers 204 with no body (via Viana.Results).
- Docs: the README shows the package's real API (new VianaResultAction(result)); ToActionResult() belongs to Viana.Results.AspNetCore.
- Added: envelope results (IEnvelopeResult<T>) are written whole, without status and problem.
