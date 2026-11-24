namespace Viana.Results.Examples.Shared.Catalog;

/// <summary>A product of the in-memory catalog.</summary>
/// <param name="Id">Identifier assigned by the catalog.</param>
/// <param name="Sku">Stock keeping unit, unique (case-insensitive), 3–20 characters.</param>
/// <param name="Name">Display name, up to 100 characters.</param>
/// <param name="Price">Unit price, greater than zero.</param>
/// <param name="Stock">Units in stock, zero or more.</param>
public sealed record Product(int Id, string Sku, string Name, decimal Price, int Stock);
