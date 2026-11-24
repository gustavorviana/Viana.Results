using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases;

// Each use case is a request (what the caller wants, and which result it gets back) and one handler.
// Handlers return Viana results: the endpoint only translates them to HTTP.

/// <summary>Lists every product.</summary>
public sealed record ListProducts : IRequest<ListResult<Product>>;

/// <summary>Gets a product by id.</summary>
public sealed record GetProduct(int Id) : IRequest<Result<Product>>;

/// <summary>Creates a product.</summary>
public sealed record CreateProduct(ProductInput Input) : IRequest<Result<Product>>;

/// <summary>Replaces a product.</summary>
public sealed record UpdateProduct(int Id, ProductInput Input) : IRequest<Result>;

/// <summary>Deletes a product without stock.</summary>
public sealed record DeleteProduct(int Id) : IRequest<Result>;

public sealed class ListProductsHandler(ProductCatalog catalog) : IHandler<ListProducts, ListResult<Product>>
{
    public Task<ListResult<Product>> Handle(ListProducts request, CancellationToken cancellationToken = default)
        => Task.FromResult(new ListResult<Product>(catalog.List()));
}

public sealed class GetProductHandler(ProductCatalog catalog) : IHandler<GetProduct, Result<Product>>
{
    public Task<Result<Product>> Handle(GetProduct request, CancellationToken cancellationToken = default)
        => Task.FromResult(catalog.Get(request.Id));
}

public sealed class CreateProductHandler(ProductCatalog catalog) : IHandler<CreateProduct, Result<Product>>
{
    public Task<Result<Product>> Handle(CreateProduct request, CancellationToken cancellationToken = default)
        => Task.FromResult(catalog.Create(request.Input));
}

public sealed class UpdateProductHandler(ProductCatalog catalog) : IHandler<UpdateProduct, Result>
{
    public Task<Result> Handle(UpdateProduct request, CancellationToken cancellationToken = default)
        => Task.FromResult(catalog.Update(request.Id, request.Input));
}

public sealed class DeleteProductHandler(ProductCatalog catalog) : IHandler<DeleteProduct, Result>
{
    public Task<Result> Handle(DeleteProduct request, CancellationToken cancellationToken = default)
        => Task.FromResult(catalog.Delete(request.Id));
}
