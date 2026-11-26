using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.EmptyResponses;

internal sealed class ProblemOnlyController : ControllerBase
{
    [ResponseExample(typeof(EmptyProblemExample))]
    [ResponseExample(typeof(EmptySuccessExample))]
    public Result<int> Get() => 1;
}
