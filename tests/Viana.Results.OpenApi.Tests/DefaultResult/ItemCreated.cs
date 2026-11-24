namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>201 with an item.</summary>
public sealed class ItemCreated : IExampleProvider<Result<Item>>
{
    public Result<Item> GetExample() => Results.Created(new Item(2, "two"));
}
