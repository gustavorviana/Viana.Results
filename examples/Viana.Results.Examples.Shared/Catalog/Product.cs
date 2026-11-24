namespace Viana.Results.Examples.Shared.Catalog;

/// <summary>A product of the in-memory catalog.</summary>
/// <param name="Id">Identifier assigned by the catalog.</param>
/// <param name="Sku">Stock keeping unit, unique (case-insensitive), 3–20 characters.</param>
/// <param name="Name">Display name, up to 100 characters.</param>
/// <param name="Price">Unit price, greater than zero.</param>
/// <param name="Stock">Units in stock, zero or more.</param>
public sealed record Product(int Id, string Sku, string Name, decimal Price, int Stock);

/// <summary>Body used to create or update a product.</summary>
public sealed class ProductInput
{
    /// <summary>Stock keeping unit (required, 3–20 characters, unique).</summary>
    /// <example>KB-001</example>
    public string? Sku { get; set; }

    /// <summary>Display name (required, up to 100 characters).</summary>
    /// <example>Mechanical keyboard</example>
    public string? Name { get; set; }

    /// <summary>Unit price (greater than zero).</summary>
    /// <example>349.90</example>
    public decimal Price { get; set; }

    /// <summary>Units in stock (zero or more).</summary>
    /// <example>10</example>
    public int Stock { get; set; }
}

/// <summary>Stock information returned by the inventory dependency.</summary>
/// <param name="ProductId">The product.</param>
/// <param name="Available">Units available.</param>
/// <param name="CheckedAt">When the inventory was queried.</param>
public sealed record StockInfo(int ProductId, int Available, DateTimeOffset CheckedAt);
