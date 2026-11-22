# Contract: Observable Behavior Changes

| ID | Package | Observable change | Version |
|----|---------|-------------------|---------|
| I1 | Viana.Results | Success with null value → body `null` (was `{}`) | 3.0.0 |
| I8 | Viana.Results | `Result<customStruct>` body unwrapped (was `{"data":...}`) | 3.0.0 |
| MV3 | Viana.Results | Failure with status 204 → problem body (was empty) | 3.0.0 |
| I9 | Viana.Results | Empty `Extensions` is read-only (cast to `Dictionary` no longer possible) | 3.0.0 |
| I10 | Viana.Results | Extension names case-insensitive: same name twice → one member, last value, first casing | 3.0.0 |
| MV1 | Viana.Results.Mvc | Failure content type `application/problem+json` (was `application/json`) | 2.1.0 |
| MV2 | Viana.Results.Mvc | Streamed writing; aborted requests stop writing (no body change) | 2.1.0 |
| MV4 | Viana.Results.Mvc | Registering twice converts once (no body change) | 2.1.0 |
| B1 | OpenApi + Swashbuckle | Generated problem examples are JSON objects with typed values | 1.1.0 / 1.1.2 |
| B2 | OpenApi + Swashbuckle | Default documented title = reason phrase | 1.1.0 / 1.1.2 |
| B4 | OpenApi + Swashbuckle | `ValueTask<T>` handled like `Task<T>` | 1.1.0 / 1.1.2 |
| B5 | OpenApi + Swashbuckle | No 401/403 on anonymous actions; added for minimal API `RequireAuthorization()` | 1.1.0 / 1.1.2 |
| M1 | OpenApi | `[Obsolete]` on internal helpers (internal in 2.0.0) | 1.1.0 |
| M2 | OpenApi | `AddVianaResultExamples` composes across calls | 1.1.0 |
| M3 | OpenApi | Example providers get constructor DI; clear error otherwise | 1.1.0 |
| M5 | OpenApi | Schema members case-insensitive like the runtime (no dropped member) | 1.1.0 |
| M6 | Swashbuckle | Unwrap keeps examples and media-type settings | 1.1.2 |

No public API is removed.
