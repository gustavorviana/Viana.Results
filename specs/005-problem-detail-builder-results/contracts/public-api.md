# Contract: Public API changes (Viana.Results 3.0.0)

```csharp
public sealed class ProblemResult
{
    // NEW
    public static ProblemResult WithDetail(
        int status, string title, string? detail = null,
        string type = "about:blank", IReadOnlyDictionary<string, object?>? extensions = null);

    // DEPRECATED (behavior unchanged)
    [Obsolete("\"description\" is not an RFC 9457 member and will be removed in Viana.Results 4.0.0. Use ProblemResult.WithDetail (RFC 9457 \"detail\") instead.")]
    public static ProblemResult WithDescription(
        int status, string title, string? description = null,
        string type = "about:blank", IReadOnlyDictionary<string, object?>? extensions = null);
}

public sealed class ProblemBuilder
{
    // DEPRECATED (behavior unchanged)
    [Obsolete("\"description\" is not an RFC 9457 member and will be removed in Viana.Results 4.0.0. Use ProblemBuilder.WithDetail (RFC 9457 \"detail\") instead.")]
    public ProblemBuilder WithDescription(string? description);

    // NEW
    public Result BuildResult();
    public Result<T> BuildResult<T>();
}
```

```csharp
// Viana.Results.OpenApi 1.1.0
public sealed class ProblemResultAttribute
{
    // NEW: documented as the RFC 9457 "detail" member of the problem schema
    public string? Detail { get; set; }

    // DEPRECATED (no effect, unchanged)
    [Obsolete("Description has no effect and will be removed in Viana.Results.OpenApi 2.0.0. Use Detail (RFC 9457 \"detail\") instead.")]
    public string? Description { get; set; }
}
```

No other public signature changes. JSON reading/writing format unchanged.
