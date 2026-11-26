namespace Viana.Results.Examples.Shared.Catalog;

/// <summary>
/// In-memory product catalog. Every operation returns a Viana result instead of throwing, so callers
/// (controllers, minimal APIs, mediator handlers) only translate it to HTTP.
/// </summary>
public sealed class ProductCatalog
{
    private readonly object _gate = new();
    private readonly Dictionary<int, Product> _products = [];
    private int _nextId;

    /// <summary>Creates the catalog seeded with three products.</summary>
    public ProductCatalog()
    {
        Seed("KB-001", "Mechanical keyboard", 349.90m, 10);
        Seed("MS-001", "Wireless mouse", 129.90m, 25);
        Seed("MN-001", "27-inch monitor", 1899.00m, 0);
    }

    /// <summary>Lists every product ordered by id.</summary>
    public IReadOnlyList<Product> List()
    {
        lock (_gate)
            return _products.Values.OrderBy(p => p.Id).ToList();
    }

    /// <summary>Gets a product, or a 404 problem.</summary>
    public Result<Product> Get(int id)
    {
        lock (_gate)
        {
            // Implicit conversion: a Product becomes a 200 Result<Product>.
            if (_products.TryGetValue(id, out var product))
                return product;
        }

        return Results.NotFound($"Product {id} not found.");
    }

    /// <summary>Creates a product: 201, a validation problem, or 409 when the SKU exists.</summary>
    public Result<Product> Create(ProductInput? input)
    {
        var validation = ProductRules.Validate(input);
        if (validation.Problem is not null)
            return validation; // a failed Result converts to Result<Product> keeping its problem

        lock (_gate)
        {
            if (SkuInUse(input!.Sku!, exceptId: null))
                return Results.Conflict($"SKU '{input.Sku}' is already in use.");

            var product = new Product(++_nextId, input.Sku!, input.Name!, input.Price, input.Stock);
            _products[product.Id] = product;
            return Results.Created(product);
        }
    }

    /// <summary>Updates a product: 204, a validation problem, 404, or 409 when the SKU belongs to another product.</summary>
    public Result Update(int id, ProductInput? input)
    {
        var validation = ProductRules.Validate(input);
        if (validation.Problem is not null)
            return validation;

        lock (_gate)
        {
            if (!_products.ContainsKey(id))
                return Results.NotFound($"Product {id} not found.");

            if (SkuInUse(input!.Sku!, exceptId: id))
                return Results.Conflict($"SKU '{input.Sku}' is already in use.");

            _products[id] = new Product(id, input.Sku!, input.Name!, input.Price, input.Stock);
            return Results.NoContent();
        }
    }

    /// <summary>Deletes a product: 204, 404, or 422 (business rule) while it still has stock.</summary>
    public Result Delete(int id)
    {
        lock (_gate)
        {
            if (!_products.TryGetValue(id, out var product))
                return Results.NotFound($"Product {id} not found.");

            if (product.Stock > 0)
                return Results.BusinessRuleViolated($"Product {id} still has {product.Stock} units in stock.");

            _products.Remove(id);
            return Results.NoContent();
        }
    }

    private bool SkuInUse(string sku, int? exceptId)
        => _products.Values.Any(p => p.Id != exceptId && string.Equals(p.Sku, sku, StringComparison.OrdinalIgnoreCase));

    private void Seed(string sku, string name, decimal price, int stock)
    {
        var product = new Product(++_nextId, sku, name, price, stock);
        _products[product.Id] = product;
    }
}
