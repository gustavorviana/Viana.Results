using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.OpenApi.Tests.EmptyResponses;

[Authorize]
internal sealed class SecuredController : ControllerBase
{
    public Result<int> Get() => 1;
}
