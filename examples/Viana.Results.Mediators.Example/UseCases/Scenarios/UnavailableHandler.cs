using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases.Scenarios;

/// <summary>Handles <see cref="UnavailableRequest"/> with the inventory answering as if it were down.</summary>
public sealed class UnavailableHandler(InventoryGateway inventory) : IHandler<UnavailableRequest, Result<StockInfo>>
{
    public Task<Result<StockInfo>> Handle(UnavailableRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(inventory.GetStock(request.ProductId, simulateOutage: true));
}
