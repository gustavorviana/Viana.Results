using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

[ProblemResult(500)]
internal sealed class OnlyInterfaceController : ControllerBase
{
    public IResult Interface() => Results.Ok();

    public ReturnUser Dto() => new();
}
