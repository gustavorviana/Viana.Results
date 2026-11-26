using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Handles <see cref="CreateProduct"/>.</summary>
public sealed class CreateProductHandler(ProductCatalog catalog) : IHandler<CreateProduct, Result<Product>>
{
    public Task<Result<Product>> Handle(CreateProduct request, CancellationToken cancellationToken = default)
        => Task.FromResult(catalog.Create(request.Input));
}
