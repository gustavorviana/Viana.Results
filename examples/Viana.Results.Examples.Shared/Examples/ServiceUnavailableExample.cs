using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>Inventory dependency down (503), with an explicit problem type and extensions.</summary>
public sealed class ServiceUnavailableExample : IExampleProvider<Result>
{
    public Result GetExample() => new ProblemBuilder(503)
        .WithTitle("Inventory unavailable")
        .WithType("https://api.example.com/errors/inventory-unavailable")
        .WithDetail("The inventory service did not answer. Retry in a few seconds.")
        .AddExtension("retryAfterSeconds", 5)
        .AddExtension("traceId", "00-9f3a2b1c4e5d6f7a8b9c0d1e2f3a4b5c-0123456789abcdef-00")
        .BuildResult();
}
