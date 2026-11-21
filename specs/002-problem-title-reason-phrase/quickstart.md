# Quickstart: Validate the Default Title

## Prerequisites
.NET SDK 10 (plus runtimes for net6.0/net8.0 test TFMs).

## Steps
1. `dotnet build -c Release` → 0 warnings, 0 errors.
2. `dotnet test -c Release` → all tests pass, including the scenarios in
   [contracts/default-title.md](contracts/default-title.md).
3. `git grep -n '"Error"' -- src tests` → only the fallback constant, fallback tests and tests that
   pass "Error" explicitly as a title remain.
4. Check `src/Viana.Results/Viana.Results.csproj`: `Version` is 2.1.0 and release notes describe the
   new default.
