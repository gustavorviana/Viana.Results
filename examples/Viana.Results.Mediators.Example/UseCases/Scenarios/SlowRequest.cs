using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases.Scenarios;

/// <summary>A slow operation that honors cancellation: aborting the call is not an error.</summary>
public sealed record SlowRequest(int Seconds) : IRequest<Result<string>>;
