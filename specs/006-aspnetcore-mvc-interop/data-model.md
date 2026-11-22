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
| other `Extensions` | `Extensions` | copied as-is; a Viana key wins over native defaults (e.g. `traceId`) |

Order: Viana values → native defaults fill only missing members → `CustomizeProblemDetails` runs last.

Success body (from `ResultResponseBody.GetBody`): null → no body; `ListResult<T>` → items array;
`PagedResult<T>` → wrapper with metadata; `Result<T>` → value.
