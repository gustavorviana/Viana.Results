using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Handles <see cref="ListProducts"/>.</summary>
public sealed class ListProductsHandler(ProductCatalog catalog) : IHandler<ListProducts, ListResult<Product>>
{
    public Task<ListResult<Product>> Handle(ListProducts request, CancellationToken cancellationToken = default)
        => Task.FromResult(new ListResult<Product>(catalog.List()));
}
