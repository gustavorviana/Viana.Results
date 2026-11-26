using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ProblemProviders;

/// <summary>Controller twin of the minimal endpoints (FR-003 parity).</summary>
[ApiController]
[Route("pp")]
public sealed class ProblemProviderController : ControllerBase
{
    [HttpGet("ctl")]
    [ResponseExample(typeof(UnavailableProblem))]
    public Result Get() => Results.Ok();
}
