using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

internal sealed class ActionResultWrappedController : ControllerBase
{
    public ActionResult<Result> Bare() => Results.Ok();

    [ProblemResult(404, Title = "Missing")]
    public ActionResult<Result<ReturnUser>> Typed() => new Result<ReturnUser>(new ReturnUser());
}
