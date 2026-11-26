using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

[ApiController]
[Route("v013/none")]
[ProblemResult(500)]
internal sealed class PipelineNoResultController : ControllerBase
{
    [HttpGet]
    public ReturnUser Dto() => new();
}
