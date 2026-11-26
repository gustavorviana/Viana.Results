using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.Examples.Shared.Examples;
using Viana.Results.Mvc;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Controllers;

/// <summary>
/// The same read through Viana.Results.Mvc (net6.0+): the action wraps the result in
/// <see cref="VianaResultAction"/>, which writes it itself. Prefer Viana.Results.AspNetCore on net8.0+
/// (native ProblemDetails, MVC formatters); keep Viana.Results.Mvc for net6.0 apps or code already using
/// it. Both packages coexist in one app. The action is declared as <c>ActionResult&lt;Result&lt;Product&gt;&gt;</c>
/// so the Viana OpenAPI declarations apply (a plain <c>IActionResult</c> does not say it returns a result).
/// </summary>
[ApiController]
[Route("api/legacy/products")]
public class LegacyProductsController(ProductCatalog catalog) : ControllerBase
{
    /// <summary>Gets a product by id (Viana.Results.Mvc).</summary>
    /// <param name="id">Product id.</param>
    [HttpGet("{id:int}", Name = "GetLegacyProduct")]
    [ProducesResponseType<Product>(StatusCodes.Status200OK)]
    [ResponseExample(typeof(ProductNotFoundExample))]
    public ActionResult<Result<Product>> Get(int id) => new VianaResultAction(catalog.Get(id));
}
