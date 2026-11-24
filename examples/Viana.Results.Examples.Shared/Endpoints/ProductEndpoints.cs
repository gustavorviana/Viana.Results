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
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblemResult<InvalidProductExample>()
            .ProducesProblemResult<ProductNotFoundExample>()
            .ProducesProblemResult<DuplicateSkuExample>();

        products.MapDelete("/{id:int}", (int id, ProductCatalog catalog) => catalog.Delete(id))
            .WithName("DeleteMinimalProduct")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblemResult<ProductNotFoundExample>()
            .ProducesProblemResult<ProductHasStockExample>();

        products.MapGet("/{id:int}/stock", (int id, InventoryGateway inventory) => inventory.GetStock(id))
            .WithName("GetMinimalProductStock")
            .ProducesResult<StockInfo>()
            .ProducesProblemResult<ProductNotFoundExample>()
            .ProducesProblemResult<ServiceUnavailableExample>();

        // Turns the fake inventory dependency on/off to reproduce the 503.
        app.MapPost("/api/inventory/availability", (bool available, InventoryGateway inventory) =>
            {
                inventory.Available = available;
                return Results.NoContent();
            })
            .AddVianaResults()
            .WithName("SetInventoryAvailability")
            .WithTags("Inventory")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}
