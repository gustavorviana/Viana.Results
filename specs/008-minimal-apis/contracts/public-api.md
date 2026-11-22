# Contract: Public API additions

```csharp
// Viana.Results.AspNetCore
namespace Viana.Results.AspNetCore;

public sealed class VianaHttpResult : Microsoft.AspNetCore.Http.IResult
{
    public VianaHttpResult(Viana.Results.IResult result);
    public Viana.Results.IResult Result { get; }
    public Task ExecuteAsync(HttpContext httpContext);
}

public static class HttpResultExtensions
{
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this Viana.Results.IResult result);
    public static Task<Microsoft.AspNetCore.Http.IResult> ToHttpResult<TResult>(this Task<TResult> result) where TResult : Viana.Results.IResult;
}

public sealed class VianaResultEndpointFilter : IEndpointFilter { }

public static class VianaEndpointConventionExtensions
{
    public static TBuilder AddVianaResults<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder;
}

// Viana.Results.OpenApi
namespace Viana.Results.OpenApi;

public sealed class VianaResponseMetadata { /* Status, DataType, ExampleProviderType, InlineExample, Title, Type, Detail, IsProblem, Name, Summary */ }

public static class VianaRouteHandlerBuilderExtensions
{
    public static TBuilder ProducesResult<T>(this TBuilder builder, string? name = null, string? summary = null);          // T: data type or IExampleProvider<>
    public static TBuilder ProducesResult(this TBuilder builder, IResult example, string? name = null, string? summary = null);
    public static TBuilder ProducesProblemResult(this TBuilder builder, int status, string? title = null, string? type = null, string? detail = null);
    public static TBuilder ProducesProblemResult<TProvider>(this TBuilder builder, string? name = null, string? summary = null);
    public static TBuilder ProducesProblemResult(this TBuilder builder, Type providerType, string? name = null, string? summary = null);
    public static TBuilder ProducesProblemResult(this TBuilder builder, IResult failedExample, string? name = null, string? summary = null);
    // TBuilder : IEndpointConventionBuilder
}

public sealed class ResponseExampleAttribute
{
    public ResponseExampleAttribute(Type providerType); // NEW: status from the provider's result
}
```
