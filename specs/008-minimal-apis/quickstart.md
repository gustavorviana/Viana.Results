# Quickstart: Validate Minimal APIs Support

1. `dotnet build Viana.Results.slnx -c Release` → 0 warnings.
2. `dotnet test Viana.Results.slnx -c Release` → all pass, including controller-vs-minimal parity and
   endpoint documentation tests (both pipelines).
3. Run the example apps: call the minimal API sample endpoint (success and failure) and compare with the
   controller endpoint; download both OpenAPI documents and check the minimal endpoint has the declared
   statuses, schemas (`VianaProblemDetails` for problems) and examples.
4. Benchmark `--filter '*AspNetCoreBenchmarks*' --job short` → `VianaHttpResult` recorded next to `VianaActionResult`.
5. Existing controller documentation unchanged (existing OpenApi tests green).
