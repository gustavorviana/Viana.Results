namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>Global 500 example.</summary>
public sealed class GlobalServerError : IExampleProvider<Result>
{
    public Result GetExample() => new ProblemBuilder(500).WithTitle("Boom").BuildResult();
}
