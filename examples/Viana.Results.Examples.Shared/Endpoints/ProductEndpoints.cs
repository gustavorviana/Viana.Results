using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Viana.Results.AspNetCore;
using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Endpoints;

/// <summary>
/// The same catalog as <c>ProductsController</c>, as minimal APIs. Handlers return Viana results;
/// <c>AddVianaResults()</c> writes them like MVC, and <c>ProducesResult</c>/<c>ProducesProblemResult</c>
/// document them.
/// </summary>
public static class ProductEndpoints
{
    /// <summary>Maps <c>/minimal/products</c> and <c>/api/inventory/availability</c>.</summary>
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/minimal/products").AddVianaResults().WithTags("Products (minimal)");

        products.MapGet("/", (ProductCatalog catalog) => new ListResult<Product>(catalog.List()))
            .WithName("ListMinimalProducts")
            .ProducesResult<Product[]>();

        products.MapGet("/{id:int}", (int id, ProductCatalog catalog) => catalog.Get(id))
            .WithName("GetMinimalProduct")
            .ProducesResult<ProductExample>()
            .ProducesProblemResult<ProductNotFoundExample>();

        products.MapPost("/", ([FromBody] ProductInput input, ProductCatalog catalog) => catalog.Create(input))
            .WithName("CreateMinimalProduct")
            .ProducesResult<ProductCreatedExample>()
            .ProducesProblemResult(400, title: "Bad Request")
            .ProducesProblemResult<InvalidProductExample>()
            .ProducesProblemResult<DuplicateSkuExample>();

        products.MapPut("/{id:int}", (int id, [FromBody] ProductInput input, ProductCatalog catalog) => catalog.Update(id, input))
            .WithName("UpdateMinimalProduct")
            .ProducesProblemResult<InvalidProductExample>()
            .ProducesProblemResult<ProductNotFoundExample>()
            .ProducesProblemResult<DuplicateSkuExample>();

        products.MapDelete("/{id:int}", (int id, ProductCatalog catalog) => catalog.Delete(id))
            .WithName("DeleteMinimalProduct")
            .ProducesProblemResult<ProductNotFoundExample>()
            .ProducesProblemResult<ProductHasStockExample>();

        products.MapGet("/{id:int}/stock", (int id, InventoryGateway inventory) => inventory.GetStock(id))
            .WithName("GetMinimalProductStock")
            .ProducesResult<StockInfo>()
            .ProducesProblemResult<ProductNotFoundExample>()
            .ProducesProblemResult<ServiceUnavailableExample>();

        // Last resort: the handler returns the common IResult but answers 200 (the product) or 202 (a reservation).
        // WithoutDefaultResult drops the inferred success; both successes are registered by hand. (Declare IResult,
        // not Result: Result<T> converted to Result keeps only the status, not the body.)
        products.MapGet("/{id:int}/reservation", IResult (int id, bool? reserve, ProductCatalog catalog) =>
            {
                var product = catalog.Get(id);
                if (product.Problem is not null || reserve != true)
                    return product;
                return new Result<Reservation>(new Reservation(id, $"R-{id:0000}"), 202);
            })
            .WithName("ReserveMinimalProduct")
            .WithoutDefaultResult()
            .ProducesResult<ProductExample>()
            .ProducesResult<ReservationAcceptedExample>()
            .ProducesProblemResult<ProductNotFoundExample>();

        // Turns the fake inventory dependency on/off to reproduce the 503.
        app.MapPost("/api/inventory/availability", (bool available, InventoryGateway inventory) =>
            {
                inventory.Available = available;
                return Results.NoContent();
            })
            .AddVianaResults()
            .WithName("SetInventoryAvailability")
            .WithTags("Inventory");

        return app;
    }
}
