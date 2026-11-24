using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>A product (200).</summary>
public sealed class ProductExample : IExampleProvider<Result<Product>>
{
    public Result<Product> GetExample() => Results.Ok(new Product(1, "KB-001", "Mechanical keyboard", 349.90m, 10));
}
