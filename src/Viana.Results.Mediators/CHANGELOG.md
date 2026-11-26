# Changelog

Versions are managed by release-please from Conventional Commits.

## [2.0.0](https://github.com/gustavorviana/Viana.Results/compare/mediators-v1.0.0...mediators-v2.0.0) (2026-11-26)


### ⚠ BREAKING CHANGES

* see each package CHANGELOG.md (Results.Ok() answers 204, problem content type, telemetry tag names, internal OpenApi helpers, OpenAPI document shape).

### Features

* first release with release-please ([#7](https://github.com/gustavorviana/Viana.Results/issues/7)) ([b2ec70b](https://github.com/gustavorviana/Viana.Results/commit/b2ec70b3c3e15abee7f8eeb91b9471905ae86928))

## 2.0.0 (upcoming) (since 1.0.0)

### Breaking changes

- Telemetry tags renamed to `viana.mediator.*` (request.type, result.type, handler.type, result.success and result.status).
- Two different handlers for the same request and result type fail at registration.
- Depends on Viana.Results 3.0.0. The telemetry status of `Results.Ok()` without a value is 204.

### Added and fixed

- Typed dispatch with no reflection after the first call. Steady-state dispatch allocates only what the handler does.
- OpenTelemetry traces and the `viana.mediator.request.duration` histogram (seconds), with options for exception detail.
- `HandlerNotFoundException` exposes `RequestType` and `ResultType`.
