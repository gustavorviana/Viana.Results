using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases.Scenarios;

/// <summary>A dependency that is down: a 503 problem, span status Error (5xx).</summary>
public sealed record UnavailableRequest(int ProductId) : IRequest<Result<StockInfo>>;
