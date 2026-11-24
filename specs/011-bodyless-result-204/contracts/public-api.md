# Public API changes

## Viana.Results
```csharp
public static Result Ok();   // behavior: status 204 (was 200). Signature unchanged.
```

## Viana.Results.OpenApi
```csharp
namespace Viana.Results.OpenApi;

/// Removes the success response inferred from the return type; only hand-registered responses are documented.
/// Last resort, for endpoints returning the base Result that can answer different successes.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class WithoutDefaultResultAttribute : Attribute { }

public static class VianaRouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder WithoutDefaultResult(this RouteHandlerBuilder builder);
    public static RouteGroupBuilder WithoutDefaultResult(this RouteGroupBuilder builder);
    public static IEndpointConventionBuilder WithoutDefaultResult(this IEndpointConventionBuilder builder);
}

// OpenApiOperationProcessor (public helper)
public static void ApplyDefaultResult(OpenApiOperation operation, MethodInfo method, IReadOnlyList<object> endpointMetadata);
// replaces ClearBodyForBareResult(operation, method)
```

## Documented behavior

| Endpoint | Success documented |
|----------|--------------------|
| returns bare `Result`, nothing declared | `204 No Content` |
| returns bare `Result`, `Produces(202)` / `[ProducesResponseType(202)]` | `202` only |
| returns `Result<T>`, nothing declared | `200` with `T` (unchanged) |
| any return, `WithoutDefaultResult`, `ProducesResult<A>()` (200) + `ProducesResult<B>(202)` | `200` and `202` only |
| any return, `WithoutDefaultResult`, nothing declared | no success response |

Problem responses are unaffected in every case.
