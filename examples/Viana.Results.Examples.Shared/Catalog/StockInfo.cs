namespace Viana.Results.Examples.Shared.Catalog;

/// <summary>Stock information returned by the inventory dependency.</summary>
/// <param name="ProductId">The product.</param>
/// <param name="Available">Units available.</param>
/// <param name="CheckedAt">When the inventory was queried.</param>
public sealed record StockInfo(int ProductId, int Available, DateTimeOffset CheckedAt);
