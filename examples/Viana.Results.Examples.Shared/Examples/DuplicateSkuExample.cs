using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>Duplicate SKU (409).</summary>
public sealed class DuplicateSkuExample : IExampleProvider<Result>
{
    public Result GetExample() => Results.Conflict("SKU 'KB-001' is already in use.");
}
