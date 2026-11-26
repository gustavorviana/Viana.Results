# Quickstart: validate the Mediators review

1. **Baseline (before code changes)**:
   `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter *MediatorBenchmarks*`
   → record in `benchmarks/Viana.Results.Benchmarks/baseline/mediators-1.0.0.md` ("before").
2. **Build**: `dotnet build -c Release` → 0 warnings.
3. **Tests**: `dotnet test` → all pass, including DispatchTests, TelemetryTests, MetricsTests,
   RegistrationTests, OptionsTests (all TFMs).
4. **Benchmark after**: same command → SC-001 (≥ 50% fewer allocations, faster than before), SC-002 (no
   listener = telemetry off), SC-006 (telemetry off ≈ direct dispatch); record "after".
5. **Docs**: README lists the real TFMs, telemetry names (see [contracts/public-api.md](contracts/public-api.md)),
   options and switches; no other mediator library is named in the README or the specs.
