# Data Model: Warnings as Errors

This feature has no runtime data. The "model" is the build policy.

## Warning

- **Code**: identifier (e.g., `CS8600`, `CS1591`, `NU1903`)
- **Source**: compiler | nullable | xml-docs | analyzer | nuget-audit
- **Project**: where it was emitted
- **TFM**: target framework it was emitted for
- **Rule**: always an error, unless covered by a Justified Exception

## Justified Exception

- **Code**: exactly one warning code
- **Scope**: one code location (`#pragma`/attribute), one project, or one folder-level props file —
  the narrowest that works
- **Justification**: inline comment next to the suppression (required)
- **Approved by**: PR reviewer

Current exceptions after this feature:

| Code | Scope | Justification |
|---|---|---|
| CS1591 | `examples/` (folder props) | Examples are not published and are not consumed as API. |

## Validation rules

- A build with any Warning not covered by a Justified Exception MUST fail (FR-001, FR-002).
- A suppression without an inline justification MUST be rejected in review (FR-005, SC-005).
