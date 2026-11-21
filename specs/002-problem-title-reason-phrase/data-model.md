# Data Model

## ProblemResult (existing, behavior change only)

| Field | Rule |
|-------|------|
| `Status` | As provided (converter: 500 when missing) |
| `Type` | Provided non-blank value kept verbatim; otherwise `"about:blank"` |
| `Title` | Provided non-blank value kept verbatim; otherwise reason phrase of `Status`; if none/placeholder, `"Error"` |

## Reason phrase (existing table in `HttpStatusInfo`)

- Source: IANA HTTP status code registry names.
- Usable: every entry except placeholders 306 and 418.
