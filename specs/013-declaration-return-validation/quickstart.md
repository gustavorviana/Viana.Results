# Quickstart: validate feature 013

## Automated

```bash
dotnet build -c Release            # 0 warnings
dotnet test tests/Viana.Results.OpenApi.Tests
dotnet test tests/Viana.Results.Examples.Tests
```

The `ReturnTypeValidation` tests cover these scenarios. For the rules behind each outcome, see
[contracts/validation-rules.md](contracts/validation-rules.md).

1. A minimal API returning `string` with `ProducesResult<User>()` fails. The same API returning `Task<Result<User>>`
   passes.
2. An action returning `UserDto` with `[ProblemResult(404)]` fails. Returning `ActionResult<Result<UserDto>>`
   passes.
3. A controller with class-level `[ProblemResult(500)]` and a mix of actions: only the result action has 500, and
   one warning is logged for the other action.
4. A controller with class-level `[ProblemResult(500)]` and no result action fails.
5. Each scenario gives the same outcome on the native pipeline and on Swashbuckle.

## Manual

Run the Scalar, Swagger and Mediators examples. Each one must open its document without errors and with no
warning from `Viana.Results.OpenApi`, because the examples use result returns only.
