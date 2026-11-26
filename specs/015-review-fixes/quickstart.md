# Quickstart: validate feature 015

```bash
dotnet build -c Release     # 0 warnings
dotnet test -c Release      # every TFM
```

1. Run the envelope tests with a conditionally written `Status`: none of the bodies contains `status`, in any
   order of instances.
2. Run the empty-responses tests: 401/403 and problem examples are documented, and success examples are still
   ignored.
3. Run the version search (research R4) outside `specs/`. It must find zero announcements.
4. Run the benchmark (`--filter *AspNetCoreBenchmarks* *MvcBenchmarks* --job short`) and compare the allocation
   with `benchmarks/Viana.Results.Benchmarks/baseline/envelope-after.md`. It must be the same or lower.
5. Build the core with `-p:IsAotCompatible=true`. It must show the same 6 pre-existing warnings and no new one.
