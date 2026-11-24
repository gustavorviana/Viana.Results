using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

// Example providers return the real Viana result: the documented status and body come from it.

/// <summary>A product (200).</summary>
public sealed class ProductExample : IExampleProvider<Result<Product>>
{
    public Result<Product> GetExample() => Results.Ok(new Product(1, "KB-001", "Mechanical keyboard", 349.90m, 10));
}

/// <summary>A created product (201).</summary>
public sealed class ProductCreatedExample : IExampleProvider<Result<Product>>
{
    public Result<Product> GetExample() => Results.Created(new Product(4, "HS-001", "Headset", 499.00m, 5));
}

/// <summary>Missing product (404).</summary>
public sealed class ProductNotFoundExample : IExampleProvider<Result>
{
    public Result GetExample() => Results.NotFound("Product 42 not found.");
}

/// <summary>Duplicate SKU (409).</summary>
public sealed class DuplicateSkuExample : IExampleProvider<Result>
{
    public Result GetExample() => Results.Conflict("SKU 'KB-001' is already in use.");
}

/// <summary>Invalid input (validation problem with an "errors" member).</summary>
public sealed class InvalidProductExample : IExampleProvider<Result>
{
    public Result GetExample() => Results.Validation(new Dictionary<string, string[]>
    {
        ["sku"] = ["The SKU is required."],
        ["price"] = ["The price must be greater than zero."]
    });
}

/// <summary>Deleting a product that still has stock (422).</summary>
public sealed class ProductHasStockExample : IExampleProvider<Result>
{
    public Result GetExample() => Results.BusinessRuleViolated("Product 1 still has 10 units in stock.");
}
