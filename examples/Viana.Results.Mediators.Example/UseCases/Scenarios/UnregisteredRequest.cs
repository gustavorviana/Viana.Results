using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Example.UseCases.Scenarios;

/// <summary>No handler is registered for this request on purpose: <see cref="HandlerNotFoundException"/>.</summary>
public sealed record UnregisteredRequest : IRequest<Result>;
