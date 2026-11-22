# Quickstart: Validate Viana.Results.AspNetCore

1. `dotnet build Viana.Results.slnx -c Release` → 0 warnings, 0 errors.
2. `dotnet test tests/Viana.Results.AspNetCore.Tests -c Release` → all pass on net8.0 and net10.0
   (parity, precedence, success formatting, filter).
3. `dotnet test Viana.Results.slnx -c Release` → old Mvc tests still pass unchanged (FR-011).
4. Run the example app and call an endpoint returning a 404 failure: the body has `type` (RFC 9110
   link), `title`, `status` and `traceId`; the content type is `application/problem+json`.
5. `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*AspNetCore*'` →
   allocations of `VianaActionResult` are not above the old `VianaResultAction` without justification.
6. `git diff --stat -- src/Viana.Results.Mvc` → empty.
