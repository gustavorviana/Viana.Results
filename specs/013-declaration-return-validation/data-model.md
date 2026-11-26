# Data Model: Validate response declarations against the return type

No persisted data. These are the internal concepts:

| Concept | Fields | Rules |
|---|---|---|
| Result return | `Type` | Viana `IResult`, or `Task<T>`/`ValueTask<T>`/`ActionResult<T>` whose `T` is a result return, checked recursively (R1). Collections and other generics are never results. |
| Declaration source | `Method`, `Class`, `EndpointMetadata` | Method and Class apply only to controllers. EndpointMetadata applies only to minimal APIs (R2, R3). |
| Validation outcome | `Apply`, `SkipWithWarning`, `Error` | See the table below. |

| Source | Endpoint returns result | Endpoint does not return result |
|---|---|---|
| Minimal metadata | Apply | Error (FR-001) |
| Action attribute | Apply | Error (FR-002) |
| Class attribute, controller has ≥1 result action | Apply | SkipWithWarning (FR-003/004) |
| Class attribute, controller has no result action | n/a | Error (FR-005) |
| Global example | Apply | Skip, silent (R7) |
