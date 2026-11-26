namespace Viana.Results.OpenApi.Tests.ProblemProviders;

/// <summary>A ProblemResult example provider (503).</summary>
public sealed class UnavailableProblem : IExampleProvider<ProblemResult>
{
    public ProblemResult GetExample() => ProblemResult.WithDetail(
        status: 503,
        title: "Unavailable",
        detail: "Try again later.",
        extensions: new Dictionary<string, object?> { ["retryAfterSeconds"] = 5 });
}
