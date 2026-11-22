# Baseline – Viana.Results.AspNetCore 1.0.0 vs Viana.Results.Mvc 2.1.0

- **Date**: 2026-11-22
- **Commit**: `7be80cb` (plus the uncommitted feature 006 work)
- **Command**: `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*AspNetCoreBenchmarks*' --job short`
- **Environment**: Windows 11, i7-11390H, .NET 10.0.12 (same machine as baseline-2.1.0.md)

| Method            | Mean     | Allocated | Alloc Ratio |
|------------------ |---------:|----------:|------------:|
| MvcSuccess        | 1.885 us |   2.67 KB |        1.00 |
| AspNetCoreSuccess | 2.334 us |   3.22 KB |        1.20 |
| MvcFailure        | 1.608 us |   2.55 KB |        0.96 |
| AspNetCoreFailure | 2.209 us |   3.63 KB |        1.36 |

**Justification (constitution Principle V)**: the extra allocations come from going through the native
ASP.NET Core pipeline instead of hand-written JSON: `ObjectResult` content negotiation and output
formatters on success, `ProblemDetails` mapping plus `IProblemDetailsService` (native defaults and
`CustomizeProblemDetails`) on failure. That pipeline is the purpose of the package (Principle IX).
Short job: means have wide error bars; allocations are the stable signal.

## Update 2026-11-22 – minimal APIs (feature 008)

| Method            | Mean     | Allocated | Alloc Ratio |
|------------------ |---------:|----------:|------------:|
| MvcSuccess        | 1.561 us |   2.67 KB |        1.00 |
| AspNetCoreSuccess | 2.134 us |   3.22 KB |        1.20 |
| MinimalSuccess    | 1.779 us |   3.00 KB |        1.12 |
| MvcFailure        | 1.242 us |   2.55 KB |        0.96 |
| AspNetCoreFailure | 1.960 us |   3.73 KB |        1.39 |
| MinimalFailure    | 2.009 us |   3.72 KB |        1.39 |

The shared problem writer (feature 008 refactor) keeps the MVC failure path within +0.1 KB of the
previous measurement (3.63 → 3.73 KB, short job). Minimal success allocates less than MVC success (no
content negotiation). Same justification as above (native pipeline).
