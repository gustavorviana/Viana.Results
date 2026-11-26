using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>Invalid input (validation problem with an "errors" member).</summary>
public sealed class InvalidProductExample : IExampleProvider<Result>
{
    public Result GetExample() => Results.Validation(new Dictionary<string, string[]>
    {
        ["sku"] = ["The SKU is required."],
        ["price"] = ["The price must be greater than zero."]
    });
}
