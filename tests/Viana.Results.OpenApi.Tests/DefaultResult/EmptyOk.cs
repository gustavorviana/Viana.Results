namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary><c>Results.Ok()</c> without value (204).</summary>
public sealed class EmptyOk : IExampleProvider<Result>
{
    public Result GetExample() => Results.Ok();
}
