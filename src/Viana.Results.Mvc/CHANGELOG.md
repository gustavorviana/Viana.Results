# Changelog

Versions are managed by release-please from Conventional Commits.

## [3.0.0](https://github.com/gustavorviana/Viana.Results/compare/mvc-v2.0.0...mvc-v3.0.0) (2026-11-26)


### ⚠ BREAKING CHANGES

* see each package CHANGELOG.md (Results.Ok() answers 204, problem content type, telemetry tag names, internal OpenApi helpers, OpenAPI document shape).

### Features

* first release with release-please ([#7](https://github.com/gustavorviana/Viana.Results/issues/7)) ([b2ec70b](https://github.com/gustavorviana/Viana.Results/commit/b2ec70b3c3e15abee7f8eeb91b9471905ae86928))

## 3.0.0 (upcoming) (since 2.0.0)

### Breaking changes

- Problem responses use `application/problem+json` (was `application/json`).
- Depends on Viana.Results 3.0.0. `Results.Ok()` answers 204, and a null value is written as `null`.

### Added and fixed

- Bodies are streamed, and writing stops when the request is aborted. `AddVianaResultFilter` is idempotent.
- Envelope results are written whole.
