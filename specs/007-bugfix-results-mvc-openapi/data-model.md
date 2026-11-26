# Data Model: Body and documentation rules after the fixes

## Response body (`ResultResponseBody.GetBody`)

| Result | Before | After |
|--------|--------|-------|
| any with `Problem != null` | problem, except status 204 → none | problem (any status) |
| no problem, status 204 | none | none |
| `Result<T>` success, T non-simple, value null | `{}` | `null` |
| `Result<T>` success, T non-simple, value set | value | value |
| `Result<T>` success, T simple | wrapper `{"data": ...}` | unchanged |
| `Result<T>` success, T custom struct | wrapper | value (unwrapped) |
| `ListResult<T>` | items | unchanged |
| `PagedResult<T>` | wrapper with metadata | unchanged |
| non-generic `Result` success | none | unchanged |

Simple types: see research R3.

## Mvc content type

| Case | Content type |
|------|--------------|
| failure | `application/problem+json` |
| success with body | `application/json` (or subclass `ContentType`) |

## OpenAPI problem example (no explicit example)

| Member | Before | After |
|--------|--------|-------|
| node kind | JSON string | JSON object |
| `status` | `"404"` | `404` |
| `title` default | `ResultMessages` sentence | reason phrase or "Error" |
| text with quotes | invalid (`""`) | valid JSON escaping |

## Extension member names (I10 / M5)

| Input order | Stored member |
|-------------|---------------|
| `errorCode`="E1", then `ErrorCode`="E2" | `errorCode` = "E2" |
| `ErrorCode`="E2", then `errorCode`="E1" | `ErrorCode` = "E1" |

Same rule in the problem, builder, JSON reading and OpenAPI schema; lookups ignore casing.
