namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>404 problem.</summary>
public sealed class ItemNotFound : IExampleProvider<Result>
{
    public Result GetExample() => Results.NotFound("Item not found");
}
