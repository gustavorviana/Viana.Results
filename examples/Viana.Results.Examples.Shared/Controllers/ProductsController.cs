using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Controllers;

/// <summary>
/// Product catalog through MVC controllers. Actions return Viana results directly; the
/// Viana.Results.AspNetCore filter (AddVianaResultFilter) writes successes with the MVC formatters and
/// failures through the native ProblemDetails pipeline.
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController(ProductCatalog catalog, InventoryGateway inventory) : ControllerBase
{
    /// <summary>Lists every product.</summary>
    [HttpGet(Name = "ListProducts")]
    public ListResult<Product> List() => new(catalog.List());

    /// <summary>Gets a product by id.</summary>
    /// <param name="id">Product id.</param>
    [HttpGet("{id:int}", Name = "GetProduct")]
    [ResponseExample(typeof(ProductExample))]
    [ResponseExample(typeof(ProductNotFoundExample))]
    public Result<Product> Get(int id) => catalog.Get(id);

    /// <summary>Creates a product.</summary>
    /// <param name="input">The product to create.</param>
    [HttpPost(Name = "CreateProduct")]
    [ResponseExample(typeof(ProductCreatedExample))]
    [ResponseExample(typeof(InvalidProductExample))]
    [ResponseExample(typeof(DuplicateSkuExample))]
    public Result<Product> Create([FromBody] ProductInput input) => catalog.Create(input);

    /// <summary>Replaces a product.</summary>
    /// <param name="id">Product id.</param>
    /// <param name="input">The new product data.</param>
    [HttpPut("{id:int}", Name = "UpdateProduct")]
    [ResponseExample(typeof(InvalidProductExample))]
    [ResponseExample(typeof(ProductNotFoundExample))]
    [ResponseExample(typeof(DuplicateSkuExample))]
    public Result Update(int id, [FromBody] ProductInput input) => catalog.Update(id, input);

    /// <summary>Deletes a product without stock.</summary>
    /// <param name="id">Product id.</param>
    [HttpDelete("{id:int}", Name = "DeleteProduct")]
    [ResponseExample(typeof(ProductNotFoundExample))]
    [ResponseExample(typeof(ProductHasStockExample))]
    public Result Delete(int id) => catalog.Delete(id);

    /// <summary>Gets the stock from the inventory dependency (503 while it is unavailable).</summary>
    /// <param name="id">Product id.</param>
    [HttpGet("{id:int}/stock", Name = "GetProductStock")]
    [ResponseExample(typeof(ProductNotFoundExample))]
    [ResponseExample(typeof(ServiceUnavailableExample), Summary = "Inventory down")]
    public Result<StockInfo> GetStock(int id) => inventory.GetStock(id);
}
