using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases.Scenarios;

// Scenarios that show how failures look in the response and in the telemetry.

/// <summary>A handler that throws: 500, span status Error and an exception event (detail level from options).</summary>
public sealed record ThrowingRequest : IRequest<Result>;

/// <summary>A dependency that is down: a 503 problem, span status Error (5xx).</summary>
public sealed record UnavailableRequest(int ProductId) : IRequest<Result<StockInfo>>;

/// <summary>A slow operation that honors cancellation: aborting the call is not an error.</summary>
public sealed record SlowRequest(int Seconds) : IRequest<Result<string>>;

/// <summary>No handler is registered for this request on purpose: <see cref="HandlerNotFoundException"/>.</summary>
public sealed record UnregisteredRequest : IRequest<Result>;

public sealed class ThrowingHandler : IHandler<ThrowingRequest, Result>
{
    public Task<Result> Handle(ThrowingRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Price table is corrupted (thrown on purpose).");
}

public sealed class UnavailableHandler(InventoryGateway inventory) : IHandler<UnavailableRequest, Result<StockInfo>>
{
    public Task<Result<StockInfo>> Handle(UnavailableRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(inventory.GetStock(request.ProductId, simulateOutage: true));
}

public sealed class SlowHandler : IHandler<SlowRequest, Result<string>>
{
    public async Task<Result<string>> Handle(SlowRequest request, CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(request.Seconds, 0, 30)), cancellationToken);
        return Results.Ok($"Finished after {request.Seconds}s.");
    }
}
