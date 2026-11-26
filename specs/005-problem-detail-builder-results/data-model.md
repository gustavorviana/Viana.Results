# Data Model

## ProblemResult extensions (unchanged storage)

| Member | Key in `Extensions` | Standard? | Set by |
|--------|---------------------|-----------|--------|
| detail | `detail` | RFC 9457 §3.1.4 | `ProblemResult.WithDetail` (new), `ProblemBuilder.WithDetail` |
| description | `description` | No (legacy) | `WithDescription` (obsolete, removed in 4.0.0) |

Rules: null/empty values are not stored; both keys may coexist; JSON reading keeps `description`.

## Builder results

| Method | Returns | Status | Problem |
|--------|---------|--------|---------|
| `Build()` | `ProblemResult` | builder status | built problem |
| `BuildResult()` | `Result` (failure) | `problem.Status` | same as `Build()` |
| `BuildResult<T>()` | `Result<T>` (failure, no value) | `problem.Status` | same as `Build()` |

## OpenAPI problem annotation

| Option | Effect on documented schema | Status |
|--------|-----------------------------|--------|
| `Status`, `Title`, `Type` | `status`, `title`, `type` members (unchanged) | current |
| `Detail` | adds `detail` string member, example = text; omitted when null/empty | new |
| `Description` | none | obsolete, removed in Viana.Results.OpenApi 2.0.0 |
