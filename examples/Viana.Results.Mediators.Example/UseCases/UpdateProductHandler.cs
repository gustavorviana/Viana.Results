using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Handles <see cref="UpdateProduct"/>.</summary>
public sealed class UpdateProductHandler(ProductCatalog catalog) : IHandler<UpdateProduct, Result>
{
    public Task<Result> Handle(UpdateProduct request, CancellationToken cancellationToken = default)
        => Task.FromResult(catalog.Update(request.Id, request.Input));
}
