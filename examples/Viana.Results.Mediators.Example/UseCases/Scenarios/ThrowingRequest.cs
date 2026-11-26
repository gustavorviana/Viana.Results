using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases.Scenarios;

/// <summary>A handler that throws: 500, span status Error and an exception event (detail level from options).</summary>
public sealed record ThrowingRequest : IRequest<Result>;
