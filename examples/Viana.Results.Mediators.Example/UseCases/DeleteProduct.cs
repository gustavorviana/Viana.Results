using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Deletes a product without stock.</summary>
public sealed record DeleteProduct(int Id) : IRequest<Result>;
