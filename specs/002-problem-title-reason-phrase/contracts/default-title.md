# Contract: Default Title

Applies to `new ProblemResult(...)`, `ProblemResult.WithDescription(...)`, `ProblemBuilder.Build()`
and JSON deserialization through `ProblemResultJsonConverter`.

| Input title | Status | Resulting title |
|-------------|--------|-----------------|
| `"Order already shipped"` | any | `"Order already shipped"` |
| null / `""` / `"  "` | 404 | `"Not Found"` |
| null / `""` / `"  "` | 500 | `"Internal Server Error"` |
| null | 409 | `"Conflict"` |
| null | 306, 418 | `"Error"` |
| null | 499, 299, 0, -1 | `"Error"` |
| missing in JSON, no status | (500) | `"Internal Server Error"` |

- Applies to every `type`, including custom URIs.
- Explicit `type` is never changed; missing/blank `type` → `"about:blank"`.
- Round trip: a defaulted title is serialized and read back unchanged.
- Public API signatures unchanged; `HttpStatusInfo.GetMessage` behavior unchanged.
