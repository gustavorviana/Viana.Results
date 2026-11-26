using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Handles <see cref="DeleteProduct"/>.</summary>
public sealed class DeleteProductHandler(ProductCatalog catalog) : IHandler<DeleteProduct, Result>
{
    public Task<Result> Handle(DeleteProduct request, CancellationToken cancellationToken = default)
        => Task.FromResult(catalog.Delete(request.Id));
}
