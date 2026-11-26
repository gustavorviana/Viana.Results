namespace Viana.Results.OpenApi.Tests.EmptyResponses;

public sealed class EmptySuccessExample : IExampleProvider<Result<int>>
{
    public Result<int> GetExample() => 7;
}
