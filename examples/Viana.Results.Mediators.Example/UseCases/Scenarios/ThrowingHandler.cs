using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases.Scenarios;

/// <summary>Handles <see cref="ThrowingRequest"/> by throwing on purpose.</summary>
public sealed class ThrowingHandler : IHandler<ThrowingRequest, Result>
{
    public Task<Result> Handle(ThrowingRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Price table is corrupted (thrown on purpose).");
}
