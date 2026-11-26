# Changelog

Versions are managed by release-please from Conventional Commits.

## 3.0.0 (upcoming) (since 2.0.0)

### Breaking changes

- Problem responses use `application/problem+json` (was `application/json`).
- Depends on Viana.Results 3.0.0. `Results.Ok()` answers 204, and a null value is written as `null`.

### Added and fixed

- Bodies are streamed, and writing stops when the request is aborted. `AddVianaResultFilter` is idempotent.
- Envelope results are written whole.
