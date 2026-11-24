using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases.Scenarios;

/// <summary>Handles <see cref="SlowRequest"/>, waiting with the request's cancellation token.</summary>
public sealed class SlowHandler : IHandler<SlowRequest, Result<string>>
{
    public async Task<Result<string>> Handle(SlowRequest request, CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(request.Seconds, 0, 30)), cancellationToken);
        return Results.Ok($"Finished after {request.Seconds}s.");
    }
}
