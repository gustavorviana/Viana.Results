namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>200 with an item.</summary>
public sealed class ItemOk : IExampleProvider<Result<Item>>
{
    public Result<Item> GetExample() => Results.Ok(new Item(1, "one"));
}
