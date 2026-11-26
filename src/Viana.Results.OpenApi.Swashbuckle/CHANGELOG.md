# Changelog

Versions are managed by release-please from Conventional Commits. The entries below were written
before release-please was adopted (feature 017) and are not yet released.

## Unreleased (since Viana.Results.OpenApi.Swashbuckle 1.1.0)

- Security: updated Microsoft.AspNetCore.OpenApi to 10.0.15 and Swashbuckle.AspNetCore to 10.3.1, which resolves the high-severity vulnerability in the transitive Microsoft.OpenApi dependency (GHSA-v5pm-xwqc-g5wc); Microsoft.OpenApi now resolves to 2.13.0.
- ProblemResultAttribute.Detail (from Viana.Results.OpenApi 1.1.0) is documented in the Swashbuckle output.
- Fixed: examples generated from [ProblemResult] are JSON objects with typed values (numbers/booleans no longer quoted, text correctly escaped).
- Fixed: the documented default problem title is the status reason phrase, matching the runtime (e.g. 404 "Not Found").
- Fixed: ValueTask<T> return types are handled like Task<T>.
- Fixed: 401/403 responses honor [AllowAnonymous] and are added for minimal API endpoints using RequireAuthorization().
- Fixed: schema member names are case-insensitive like the runtime (no member silently dropped).
- Fixed: unwrapping Result<T> response schemas keeps existing examples and media-type settings.
- Changed: problem responses are documented like the native ProblemDetails: a shared "VianaProblemDetails" component (type, title, status, detail, instance; optional and nullable) referenced with $ref, plus allOf for extra members; content type application/problem+json only.
- Changed: one source per status for schema and example: endpoint [ResponseExample] > action [ProblemResult] > controller [ProblemResult] > global example (fallback only); several endpoint examples are merged.
- Fixed: an action [ProblemResult] now wins over the controller one for the same status.
- Changed: extension member names follow the application JSON naming policy, like the runtime.
- Added: ProducesResult / ProducesProblemResult for minimal API endpoints (data type, example class, inline result, problem metadata), read by both OpenAPI pipelines.
- Added: example providers may return real Viana results (Result<T>, ListResult<T>, PagedResult<T>, failed Result), written like the runtime body; [ResponseExample(typeof(Provider))] without status.
- Fixed: a documented problem without its own type shows the native RFC 9110 link the runtime sends (about:blank only for statuses without a native default).
- Same success documentation as Viana.Results.OpenApi: bare Result documented as 204, hand-declared successes replace the inferred one, [WithoutDefaultResult] / WithoutDefaultResult() supported.
- Fixed: ProblemResult example providers on minimal APIs are documented as problems (via Viana.Results.OpenApi).
- Changed: same return-type validation as Viana.Results.OpenApi (declarations need a Viana result return; class-level ones skip non-result actions with a warning). ProblemResponseOperationFilter takes an optional IServiceProvider for the warning logger.
- Added: same envelope result documentation as Viana.Results.OpenApi.
- Fixed: 401/403 and problem examples are documented on operations without responses (via Viana.Results.OpenApi).
