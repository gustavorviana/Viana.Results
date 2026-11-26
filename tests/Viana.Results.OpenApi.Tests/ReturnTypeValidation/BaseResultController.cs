using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

[ProblemResult(500)]
internal abstract class BaseResultController : ControllerBase
{
    public Result<ReturnUser> Inherited() => new ReturnUser();

    public ReturnUser InheritedDto() => new();
}
