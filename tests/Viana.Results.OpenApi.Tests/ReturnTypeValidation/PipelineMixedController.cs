using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

[ApiController]
[Route("v013/mixed")]
[ProblemResult(500)]
internal sealed class PipelineMixedController : ControllerBase
{
    [HttpGet("result")]
    public Result<ReturnUser> Typed() => new ReturnUser();

    [HttpGet("dto")]
    public ReturnUser Dto() => new();
}
