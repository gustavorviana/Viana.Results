# Research: Problem Title Defaults to Status Reason Phrase

## R1 – Where to apply the default
- **Decision**: In the private `ProblemResult` constructor only.
- **Rationale**: Constructor, `WithDescription`, `ProblemBuilder` (passes `_title ?? string.Empty`)
  and `ProblemResultJsonConverter` (passes `title ?? string.Empty`) all reach it; one change point
  guarantees FR-002/SC-004.
- **Alternatives**: Defaulting in each entry point (duplication, drift risk — removed in feature 001).

## R2 – Reason phrase source
- **Decision**: Add `internal static bool TryGetReasonPhrase(int code, out string phrase)` to
  `HttpStatusInfo`, reading the existing table and rejecting placeholder entries (306 "Unused",
  418 "(Unused)").
- **Rationale**: Reuses the IANA table already maintained; public `GetMessage` keeps returning
  "Unknown" for unknown codes (no public behavior change).
- **Alternatives**: Calling `GetMessage` and comparing against "Unknown" (stringly-typed, leaks
  placeholders); a second table (duplication).

## R3 – Fallback
- **Decision**: `"Error"` for unknown, placeholder, zero or negative statuses.
- **Rationale**: FR-003, SC-002; matches previous default so no empty titles.

## R4 – Versioning
- **Decision**: `Viana.Results` 2.0.0 → 2.1.0 (MINOR), release notes call out the new default.
- **Rationale**: Owner decision (spec Clarifications); RFC 9457 treats `title` as advisory.
  Satellites need no version bump (no code change); their tests are updated only if they assert "Error".

## R5 – Configurability
- **Decision**: Not configurable in core (FR-008).
- **Rationale**: Principles I/V; global mutable state is order-dependent. Host frameworks customize
  via `CustomizeProblemDetails` (Principle IX).
