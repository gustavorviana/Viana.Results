# Research: Clean Up Unused Central Package Entries

## R1 – How to verify an entry is unused
- **Decision**: Search every `*.csproj`, `*.props` and `*.targets` (excluding bin/obj) for
  `PackageReference`/`GlobalPackageReference` of each candidate; confirm no lock files.
- **Result (2026-11-21)**: Microsoft.AspNet.Mvc, Microsoft.AspNetCore.Mvc.Core, Newtonsoft.Json and
  Swashbuckle.AspNetCore.SwaggerGen have zero references. The only mention is historical release
  notes in `src/Viana.Results.Mvc/Viana.Results.Mvc.csproj` (text, not a reference).
- **Alternatives**: Remove and rely on restore errors only (misses entries that are silently unused,
  since CPM does not warn about unused `PackageVersion`).

## R2 – Transitive pinning
- **Decision**: Nothing to preserve; `CentralPackageTransitivePinningEnabled` is not set, so
  `PackageVersion` entries without a reference have no effect on the graph.
- **Rationale**: Removing them cannot change any resolved version (FR-003, FR-005).

## R3 – Swashbuckle.AspNetCore.SwaggerGen vs Swashbuckle.AspNetCore
- **Decision**: Remove only `SwaggerGen` (8.0.0); keep `Swashbuckle.AspNetCore` (10.3.1), which is
  referenced and brings SwaggerGen transitively at its own version.
