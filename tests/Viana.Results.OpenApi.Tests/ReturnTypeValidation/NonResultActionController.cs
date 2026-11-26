using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

internal sealed class NonResultActionController : ControllerBase
{
    [ProblemResult(404)]
    public ReturnUser ProblemOnDto() => new();

    [ResponseExample(typeof(NotFoundProblemExample))]
    public IActionResult ProblemExampleOnActionResult() => Ok();

    [ResponseExample(typeof(OkUserExample))]
    public Task SuccessExampleOnTask() => Task.CompletedTask;

    [WithoutDefaultResult]
    public ReturnUser WithoutDefaultOnDto() => new();

    [ProblemResult(404)]
    public Result<ReturnUser> ProblemOnResult() => new ReturnUser();
}
