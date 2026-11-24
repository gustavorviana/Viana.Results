namespace Viana.Results.Examples.Shared.Catalog;

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
