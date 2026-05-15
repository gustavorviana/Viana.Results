using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>Global 500 example registered via VianaResultOptions.</summary>
public sealed class InternalServerErrorExample : IExampleProvider<ProblemResult>
{
    public ProblemResult GetExample() => ProblemResult.WithDescription(
        status: 500,
        title: "Internal Server Error",
        description: "An unexpected error occurred. The incident has been logged.",
        type: "https://api.example.com/errors/internal",
        extensions: new Dictionary<string, object?>
        {
            ["traceId"] = "00-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa-bbbbbbbbbbbbbbbb-00"
        });
}
