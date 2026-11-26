using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

[ProblemResult(500)]
internal sealed class NoResultController : ControllerBase
{
    public ReturnUser Dto() => new();

    public Result Property => Results.Ok();

    [NonAction]
    public Result Helper() => Results.Ok();
}
