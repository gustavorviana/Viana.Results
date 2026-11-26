using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.AspNetCore.Tests.Envelopes;

[ApiController]
[Route("envelope")]
public sealed class EnvelopeController : ControllerBase
{
    internal static AspNetCursorResult Sample()
        => new([1, 2], "abc", new Dictionary<string, object?> { ["region"] = "eu", ["items"] = "ignored" });

    [HttpGet("action")]
    public IActionResult Action() => Sample().ToActionResult();

    [HttpGet("direct")]
    public AspNetCursorResult Direct() => Sample();
}
