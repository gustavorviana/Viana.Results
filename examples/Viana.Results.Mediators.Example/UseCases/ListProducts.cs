using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

/// <summary>Lists every product.</summary>
public sealed record ListProducts : IRequest<ListResult<Product>>;
