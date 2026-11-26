using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

[ApiController]
[Route("v013/action")]
internal sealed class PipelineActionController : ControllerBase
{
    [HttpGet]
    [ProblemResult(404)]
    public ReturnUser Get() => new();
}
