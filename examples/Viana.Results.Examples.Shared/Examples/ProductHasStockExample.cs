using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>Deleting a product that still has stock (422).</summary>
public sealed class ProductHasStockExample : IExampleProvider<Result>
{
    public Result GetExample() => Results.BusinessRuleViolated("Product 1 still has 10 units in stock.");
}
