using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Replaces a product.</summary>
public sealed record UpdateProduct(int Id, ProductInput Input) : IRequest<Result>;
