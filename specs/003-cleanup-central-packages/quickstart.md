# Quickstart: Validate the Cleanup

1. `git grep -n -E 'Microsoft.AspNet.Mvc"|Mvc.Core"|Newtonsoft.Json"|SwaggerGen"' -- Directory.Packages.props` → no output.
2. For each remaining `PackageVersion`, `git grep -l 'PackageReference Include="<name>"' -- '*.csproj'` → at least one hit.
3. `dotnet restore` → no NU1xxx warnings.
4. `dotnet build -c Release` → 0 warnings, 0 errors.
5. `dotnet test -c Release` → all tests pass.
6. `git diff --stat` → only `Directory.Packages.props` changed (besides specs).
