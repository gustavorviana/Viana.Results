# Quickstart: Validate the Bug-fix Sweep

1. `dotnet build Viana.Results.slnx -c Release` → 0 warnings, 0 errors.
2. `dotnet test Viana.Results.slnx -c Release` → all pass; each item of
   [contracts/behavior-changes.md](contracts/behavior-changes.md) has a regression test.
3. Run the example apps (Scalar and Swagger), download the OpenAPI documents and check:
   problem examples are objects with numeric `status`; default titles are reason phrases;
   anonymous actions have no 401/403.
4. `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*MvcBenchmarks*'`
   → `VianaResultAction` allocates less than before (record before/after in the PR).
5. `git diff --stat -- src/Viana.Results.Mediators` → empty (out of scope).
6. Release notes updated: core 3.0.0, Mvc 2.1.0, OpenApi 1.1.0, Swashbuckle 1.1.2.
