namespace Viana.Results.Examples.Shared.Catalog;

/// <summary>
/// Fake external inventory service. Turn it off (POST /api/inventory/availability?available=false) to see
/// a 503 problem built with <see cref="ProblemBuilder"/>.
/// </summary>
public sealed class InventoryGateway(ProductCatalog catalog)
{
    /// <summary>Whether the dependency answers.</summary>
    public bool Available { get; set; } = true;

    /// <summary>Gets the stock of a product: 200, 404, or 503 while unavailable.</summary>
    /// <param name="id">Product id.</param>
    /// <param name="simulateOutage">Answers as if the dependency were down, for this call only.</param>
    public Result<StockInfo> GetStock(int id, bool simulateOutage = false)
    {
        if (!Available || simulateOutage)
        {
            return new ProblemBuilder(503)
                .WithTitle("Inventory unavailable")
                .WithDetail("The inventory service did not answer. Retry in a few seconds.")
                .AddExtension("retryAfterSeconds", 5)
                .BuildResult<StockInfo>();
        }

        var product = catalog.Get(id);
        if (product.Problem is not null)
            return product.Problem; // ProblemResult converts implicitly to Result<StockInfo>

        return new StockInfo(id, product.Data!.Stock, DateTimeOffset.UtcNow);
    }
}
