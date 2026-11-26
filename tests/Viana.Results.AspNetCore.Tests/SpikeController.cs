using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.AspNetCore.Tests;

[ApiController]
[Route("spike")]
public sealed class SpikeController : ControllerBase
{
    [HttpGet]
    public async Task Get([FromServices] IProblemDetailsService problems)
    {
        HttpContext.Response.StatusCode = 404;
        HttpContext.Response.Headers["x-spike"] = "hit";
        await problems.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = HttpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = 404,
                Title = "Viana title",
                Type = null,
                Extensions = { ["traceId"] = "viana-trace" }
            }
        });
    }
}
