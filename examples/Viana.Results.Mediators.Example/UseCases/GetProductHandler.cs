using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Handles <see cref="GetProduct"/>.</summary>
public sealed class GetProductHandler(ProductCatalog catalog) : IHandler<GetProduct, Result<Product>>
{
    public Task<Result<Product>> Handle(GetProduct request, CancellationToken cancellationToken = default)
        => Task.FromResult(catalog.Get(request.Id));
}
