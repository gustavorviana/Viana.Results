# Data Model: Envelope results

| Type | Kind | Notes |
|---|---|---|
| `IEnvelopeResult<T>` | public interface, `: IResult<T>` | Marker. There are no new members. |
| `PagedResult<T>` | existing class | Adds `IEnvelopeResult<IReadOnlyList<T>>`. The body stays `{ data, pageNumber, totalPages }`. |
| `CursorResult<T>` | example type (examples only) | `Data`, `NextCursor`. Implements `IEnvelopeResult<IReadOnlyList<T>>`. |

## Body rules (envelope)

| Member | Written? |
|---|---|
| Public properties in the JSON contract | yes (JSON names from the application options / `[JsonPropertyName]`) |
| `[JsonIgnore]` properties | no |
| `IResult.Status`, `IResult.Problem` | never |
| `IHasExtensions.Extensions` | never as a member. Its entries go to the root, and a declared member wins on a name clash (case-insensitive). |

## Precedence

1. A result with a problem: the problem body.
2. Status 204: no body.
3. An envelope: the envelope body above.
4. Otherwise: today's rules (unwrap `Data`, scalars wrapped, non-generic `Result` without a body).
