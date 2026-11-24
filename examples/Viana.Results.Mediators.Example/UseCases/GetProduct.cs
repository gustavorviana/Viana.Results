using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Gets a product by id.</summary>
public sealed record GetProduct(int Id) : IRequest<Result<Product>>;
