# Changelog

Versions are managed by release-please from Conventional Commits.

## 1.0.0 (2026-11-26)


### ⚠ BREAKING CHANGES

* see each package CHANGELOG.md (Results.Ok() answers 204, problem content type, telemetry tag names, internal OpenApi helpers, OpenAPI document shape).

### Features

* first release with release-please ([#7](https://github.com/gustavorviana/Viana.Results/issues/7)) ([b2ec70b](https://github.com/gustavorviana/Viana.Results/commit/b2ec70b3c3e15abee7f8eeb91b9471905ae86928))

## 1.0.0 (upcoming)

### Added and fixed

- First release: controllers (`ToActionResult()`, `AddVianaResultFilter()`) and minimal APIs (`ToHttpResult()`, `AddVianaResults()`) through the native pipeline: ProblemDetails, `traceId`, `CustomizeProblemDetails`, and the MVC formatters for successes.
- Envelope results are written whole with the application JSON options.
