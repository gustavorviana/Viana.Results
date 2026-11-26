# Data Model

## VianaResponseMetadata (one per declaration on an endpoint)

| Field | Meaning |
|-------|---------|
| `Status` | explicit status (nullable when taken from an example result) |
| `DataType` | documented success type (`ProducesResult<T>` with a data type) |
| `ExampleProviderType` | example provider class |
| `InlineExample` | inline Viana result instance |
| `Title`, `Type`, `Detail` | problem metadata (`ProducesProblemResult(status, ...)`) |
| `IsProblem` | declared as a problem response |
| `Name`, `Summary` | example naming (several examples per status) |

Rules: status from the example result wins; explicit status ≠ result status → error. Body of a result
example = `ResultResponseBody.GetBody(result)`.

## Runtime parity (controller vs minimal)

| Result | Controller | Minimal |
|--------|------------|---------|
| failure | native problem (006) | same |
| success with value | MVC formatters, status | minimal JSON, status |
| no body | status only | status only |
| null value | 204 (MVC default) | 204 (aligned, R2) |
