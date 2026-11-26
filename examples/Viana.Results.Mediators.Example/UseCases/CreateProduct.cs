using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Creates a product.</summary>
public sealed record CreateProduct(ProductInput Input) : IRequest<Result<Product>>;
