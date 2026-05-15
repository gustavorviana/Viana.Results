using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

public sealed class ServiceUnavailableExample : IExampleProvider<ProblemResult>
{
    public ProblemResult GetExample() => ProblemResult.WithDescription(
        status: 503,
        title: "Service Unavailable",
        description: "Downstream forecast provider is unreachable. Retry in a few seconds.",
        type: "https://api.example.com/errors/service-unavailable",
        extensions: new Dictionary<string, object?>
        {
            ["retryAfterSeconds"] = 5,
            ["traceId"] = "00-9f3a2b1c4e5d6f7a8b9c0d1e2f3a4b5c-0123456789abcdef-00"
        });
}
