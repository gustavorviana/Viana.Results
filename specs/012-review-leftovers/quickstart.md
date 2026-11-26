# Quickstart: validate

1. Run `dotnet build -c Release`. Expect 0 warnings. The Mvc README snippets compile as part of `Viana.Results.Mvc.Tests`.
2. Run `dotnet test`. Expect all tests to pass, including the problem-provider tests (both generators) and the duration advice test on net10.
3. Run `dotnet run --project examples/Viana.Results.Scalar` and open `/openapi/v1.json`. `GET /minimal/products/{id}/stock` lists 503 from `ServiceUnavailableExample`, which is an `IExampleProvider<ProblemResult>` again.
4. Run `dotnet run --project examples/Viana.Results.Mediators.Example` and call `GET /products/1`. The console metric shows the count in the `(0,0.005]` or `(0.005,0.01]` bucket.
