using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>Missing product (404).</summary>
public sealed class ProductNotFoundExample : IExampleProvider<Result>
{
    public Result GetExample() => Results.NotFound("Product 42 not found.");
}
