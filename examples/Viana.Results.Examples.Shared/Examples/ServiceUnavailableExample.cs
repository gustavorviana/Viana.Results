using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>
/// Inventory dependency down (503), as a <see cref="ProblemResult"/> provider with an explicit problem type and
/// extensions. Works on controllers ([ResponseExample]) and minimal APIs (ProducesProblemResult) alike.
/// </summary>
public sealed class ServiceUnavailableExample : IExampleProvider<ProblemResult>
{
    public ProblemResult GetExample() => ProblemResult.WithDetail(
        status: 503,
        title: "Inventory unavailable",
        detail: "The inventory service did not answer. Retry in a few seconds.",
        type: "https://api.example.com/errors/inventory-unavailable",
        extensions: new Dictionary<string, object?>
        {
            ["retryAfterSeconds"] = 5,
            ["traceId"] = "00-9f3a2b1c4e5d6f7a8b9c0d1e2f3a4b5c-0123456789abcdef-00"
        });
}
