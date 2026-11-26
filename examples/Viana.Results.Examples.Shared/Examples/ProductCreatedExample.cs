using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>A created product (201).</summary>
public sealed class ProductCreatedExample : IExampleProvider<Result<Product>>
{
    public Result<Product> GetExample() => Results.Created(new Product(4, "HS-001", "Headset", 499.00m, 5));
}
