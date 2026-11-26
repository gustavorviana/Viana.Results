# Changelog

Versions are managed by release-please from Conventional Commits.

## [2.0.0](https://github.com/gustavorviana/Viana.Results/compare/openapi-v1.0.0...openapi-v2.0.0) (2026-11-26)


### ⚠ BREAKING CHANGES

* see each package CHANGELOG.md (Results.Ok() answers 204, problem content type, telemetry tag names, internal OpenApi helpers, OpenAPI document shape).

### Features

* first release with release-please ([#7](https://github.com/gustavorviana/Viana.Results/issues/7)) ([b2ec70b](https://github.com/gustavorviana/Viana.Results/commit/b2ec70b3c3e15abee7f8eeb91b9471905ae86928))

## 2.0.0 (upcoming) (since 1.0.0)

### Breaking changes

- The helpers `OpenApiOperationProcessor`, `OpenApiDocumentProcessor`, `OpenApiSchemaProcessor`, `ResponseExampleResolver`, `ResolvedExample`, `ProblemFieldSchema` and `ProblemResultSchema` are internal. `ClearBodyForBareResult` was removed, and `NoResponseBodyOperationTransformer` has no parameterless constructor.
- A bare `Result` is documented as 204. Hand-declared successes replace the inferred one.
- Viana declarations on endpoints that do not return a Viana result fail document generation. On a controller class they are skipped for those actions, with a warning.
- Extension member names follow the application JSON naming policy, and the example precedence changed (endpoint example > action > controller > global).

### Added and fixed

- `ProducesResult` / `ProducesProblemResult` for minimal APIs, and example providers that return real results.
- `[WithoutDefaultResult]` / `WithoutDefaultResult()` as a last resort, and envelope result schemas that match the runtime.
- Problem responses share a `VianaProblemDetails` component, with one source per status.
