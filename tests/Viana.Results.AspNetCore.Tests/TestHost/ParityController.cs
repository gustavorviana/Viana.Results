using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.AspNetCore.Tests.TestHost;

[ApiController]
[Route("parity")]
public sealed class ParityController : ControllerBase
{
    [HttpGet("native/{status:int}")]
    public IActionResult Native(int status, string? title, string? type, string? detail)
        => Problem(statusCode: status, title: title, type: type, detail: detail);

    [HttpGet("viana/{status:int}")]
    public IActionResult Viana(int status, string? title, string? type, string? detail)
    {
        var builder = new ProblemBuilder(status).WithTitle(title).WithType(type);
        if (detail != null)
            builder.WithDetail(detail);
        return builder.BuildResult().ToActionResult();
    }
}
