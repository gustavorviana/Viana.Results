# Contract: Public API (Viana.Results.AspNetCore 1.0.0)

```csharp
namespace Viana.Results.AspNetCore;

public sealed class VianaActionResult : IActionResult   // wraps any Viana IResult
{
    public VianaActionResult(IResult result);
    public IResult Result { get; }
    public Task ExecuteResultAsync(ActionContext context);
}

public static class ResultActionResultExtensions
{
    public static IActionResult ToActionResult(this IResult result);
    public static Task<IActionResult> ToActionResult<TResult>(this Task<TResult> result) where TResult : IResult;
}

public static class VianaResultFilterExtensions
{
    public static IMvcBuilder AddVianaResultFilter(this IMvcBuilder builder); // opt-in global filter
}
```

Behavior: see [problem-mapping.md](problem-mapping.md) and spec FR-002 to FR-009.
