using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

[ProblemResult(500)]
[ResponseExample(typeof(NotFoundProblemExample))]
internal sealed class MixedController : ControllerBase
{
    public Result<ReturnUser> Typed() => new ReturnUser();

    public IResult Interface() => Results.Ok();

    public ReturnUser Dto() => new();

    [NonAction]
    public Result Helper() => Results.Ok();
}
