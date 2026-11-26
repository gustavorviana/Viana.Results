using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>A cursor page: the example is written exactly like the runtime envelope body.</summary>
public sealed class ProductCursorExample : IExampleProvider<CursorResult<Product>>
{
    /// <inheritdoc />
    public CursorResult<Product> GetExample()
        => new([new Product(1, "KB-001", "Mechanical keyboard", 349.90m, 10)], "eyJpZCI6MX0");
}
