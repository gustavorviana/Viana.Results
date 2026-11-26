namespace Viana.Results.OpenApi.Tests.ProblemProviders;

/// <summary>A provider that is neither a Viana result nor a ProblemResult.</summary>
public sealed class NotAResult : IExampleProvider<string>
{
    public string GetExample() => "x";
}
