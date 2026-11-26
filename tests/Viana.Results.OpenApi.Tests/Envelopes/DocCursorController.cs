using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.Envelopes;

[ApiController]
[Route("v014")]
internal sealed class DocCursorController : ControllerBase
{
    [HttpGet("cursor")]
    [ResponseExample(typeof(DocCursorExample))]
    public DocCursorResult Cursor() => new DocCursorExample().GetExample();

    [HttpGet("extended")]
    public ExtendedCursorResult Extended() => new([1], new Dictionary<string, object?>());
}
