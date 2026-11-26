# Quickstart: Validate Core Benchmarks

1. `dotnet build Viana.Results.slnx -c Release` → 0 warnings, 0 errors (benchmarks project included).
2. `dotnet test Viana.Results.slnx -c Release --no-build` → same test count as before; no benchmark runs.
3. `dotnet pack Viana.Results.slnx -c Release -o <tmp>` → no `Viana.Results.Benchmarks*.nupkg`.
4. `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*ProblemBenchmarks*' --job short`
   → table with Mean and Allocated for the 4 problem benchmarks only.
5. Open `benchmarks/Viana.Results.Benchmarks/baseline/baseline-2.1.0.md` → all 14 benchmarks of
   [contracts/benchmark-catalog.md](contracts/benchmark-catalog.md) with environment header.
6. Follow `benchmarks/README.md` "Compare" section end to end.
