# Contract: public API (feature 014)

## Added

```csharp
namespace Viana.Results;

/// A result whose whole object (minus Status, Problem and the Extensions container) is the response body.
public interface IEnvelopeResult<out T> : IResult<T> { }

public static class ResultResponseBody
{
    // existing: object? GetBody(IResult? result)
    public static object? GetBody(IResult? result, JsonSerializerOptions options);   // NEW
}
```

## Changed

- `PagedResult<T>` implements `IEnvelopeResult<IReadOnlyList<T>>`, and its output is unchanged.
- The `IPagedResult<>` special case is removed from the unwrap rule. Behavior is the same, because the type is now
  an envelope.

## Behavior guarantees

- The runtime body and the OpenAPI schema have the same members for every envelope, in every package.
- `[JsonPropertyName]` and the naming policy are honored on both sides.
- Problems and 204 are unchanged.
