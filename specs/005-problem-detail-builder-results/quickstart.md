# Quickstart: Validate "detail" and Builder Results

1. `dotnet build Viana.Results.slnx -c Release` → 0 warnings, 0 errors (no internal obsolete use).
2. `dotnet test Viana.Results.slnx -c Release` → all pass (obsolete behaviors unchanged, new tests green).
3. `git grep -n "WithDescription" -- src examples` → only the obsolete declarations and their XML docs.
4. `git grep -n "pragma warning disable CS0618" -- tests` → each occurrence wraps an obsolete call and has a comment.
5. (Optional; `ObsoleteApiTests` is the official check) A scratch file calling
   `ProblemResult.WithDescription(...)` produces CS0618 with the 4.0.0 removal message from
   [contracts/public-api.md](contracts/public-api.md).
6. `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*ProblemBenchmarks*'`
   → existing benchmarks within noise of `baseline/baseline-2.1.0.md`; `BuilderBuildResult` recorded.
7. `src/Viana.Results/Viana.Results.csproj` → Version 3.0.0 with release notes for 002 and 005.
8. Generate the OpenAPI document of the example app (Scalar and Swagger hosts): an endpoint with
   `[ProblemResult(400, Detail = "...")]` shows a `detail` member with that example; endpoints without
   `Detail` are unchanged.
9. `Viana.Results.OpenApi.csproj` → 1.1.0; `Viana.Results.OpenApi.Swashbuckle.csproj` → 1.1.2, with release notes.
