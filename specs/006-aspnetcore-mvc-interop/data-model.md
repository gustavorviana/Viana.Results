# Data Model: Problem Mapping

A failure is always a Viana `IResult` (`Result`, `Result<T>`, `ListResult<T>`, `PagedResult<T>`) whose
`Problem` property holds the `ProblemResult`. Only `result.Problem` is mapped; `Data` is ignored on
failure. Failure ⇔ `result.Problem != null`.

**Status rule**: `result.Status` is the HTTP status code AND the `status` member of the body
(`ProblemResult.Status` is not used for the response), so both never diverge (RFC 9457 §3.1.2).

| Viana source | Native `ProblemDetails` | Rule |
|-----------------------|-------------------------|------|
| `result.Status` | `Status` | always from the result, not from the problem |
| `Title` | `Title` | always copied (the core already defaults it to the reason phrase) |
| `Type` | `Type` | `about:blank` → `null` (native default link applies); otherwise copied |
| `Extensions["detail"]` | `Detail` | promoted, removed from extensions |
| `Extensions["instance"]` | `Instance` | promoted, removed from extensions |
| `Extensions["description"]` (obsolete) | `Extensions["description"]` | copied as an extension; only `detail` is promoted |
| other `Extensions` | `Extensions` | copied into a case-insensitive dictionary with names converted by the MVC JSON `PropertyNamingPolicy`; last value wins on collisions; a Viana key wins over native defaults, **except `traceId`** (any casing): removed before the native pipeline runs and restored only if the native pipeline did not set one |

Order: Viana values → native defaults fill only missing members → `CustomizeProblemDetails` runs last.

Success body (from `ResultResponseBody.GetBody`): null → no body; `ListResult<T>` → items array;
`PagedResult<T>` → wrapper with metadata; `Result<T>` → value.

## Null success value

`Result<T>` success with a null value → `ObjectResult(null)` → MVC default: 204 without body
(`HttpNoContentOutputFormatter`, `TreatNullValueAsNoContent = true`). Old MVC package: `null` with the
result status (unchanged).

## Extension names per package

| Package | Names written |
|---------|---------------|
| Viana.Results (converter) | serializer `PropertyNamingPolicy` applied |
| Viana.Results.AspNetCore | MVC `JsonOptions` `PropertyNamingPolicy` applied |
| Viana.Results.OpenApi / Swashbuckle | same policy as the runtime JSON options |
| Viana.Results.Mvc (old) | as stored (original rule, via an internal "preserve names" converter mode) |
| Newtonsoft apps | as stored (future dedicated package) |
