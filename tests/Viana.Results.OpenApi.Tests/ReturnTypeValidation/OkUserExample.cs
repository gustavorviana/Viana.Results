namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

public sealed class OkUserExample : IExampleProvider<Result<ReturnUser>>
{
    public Result<ReturnUser> GetExample() => new ReturnUser { Id = 1 };
}
