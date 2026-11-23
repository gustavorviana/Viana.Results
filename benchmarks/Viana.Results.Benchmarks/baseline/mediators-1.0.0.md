# Baseline – Viana.Results.Mediators 1.0.0 (before feature 009)

- **Date**: 2026-11-23
- **Commit**: `db5b261` (dispatch code unchanged)
- **Command**: `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*MediatorBenchmarks*' --job short`
- **Environment**: Windows 11, i7-11390H, .NET 10 (same machine as the other baselines)
- `Send_NewMediator`: a new mediator per call, like one mediator per request scope (the registered lifetime).

| Method            | Mean      | Allocated | Alloc Ratio |
|------------------ |----------:|----------:|------------:|
| DirectHandler     |  12.82 ns |     104 B |        1.00 |
| Send              | 180.93 ns |     480 B |        4.62 |
| Send_NewMediator  | 619.74 ns |    1480 B |       14.23 |
| Send_WithListener | 526.49 ns |    1136 B |       10.92 |

## After feature 009 (2026-11-23)

| Method            | Mean       | Allocated | Alloc Ratio |
|------------------ |-----------:|----------:|------------:|
| DirectHandler     |   9.505 ns |     104 B |        1.00 |
| Send              |  76.269 ns |     104 B |        1.00 |
| Send_NewMediator  |  70.890 ns |     192 B |        1.85 |
| Send_WithListener | 502.946 ns |     928 B |        8.92 |
| Send_TelemetryOff |  64.795 ns |     104 B |        1.00 |

- **SC-001**: `Send` 480 B → 104 B (−78%), 181 ns → 76 ns; `Send_NewMediator` (one mediator per scope)
  1480 B → 192 B (−87%), 620 ns → 71 ns. Steady-state dispatch allocates only what the handler itself does.
- **SC-002 / SC-006**: without a listener, and with both kinds disabled while listeners are active,
  dispatch costs the same (104 B, within noise).
- `Send_WithListener` (1136 B → 928 B) is the observed path: span, tags and histogram measurement, paid only
  when a listener samples the dispatch. `Send_NewMediator` keeps 88 B for the mediator instance itself.
- Short job: means have wide error bars; allocations are the stable signal.
