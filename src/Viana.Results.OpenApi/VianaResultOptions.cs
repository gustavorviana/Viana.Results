using System;
using System.Collections.Generic;

namespace Viana.Results.OpenApi;

/// <summary>
/// Configuration for cross-cutting Viana.Results OpenAPI behavior, currently used to register
/// global response example providers that apply to any action that returns the matching status.
/// </summary>
public sealed class VianaResultOptions
{
    private readonly List<GlobalExampleRegistration> _examples = new();

    /// <summary>The list of globally-registered examples.</summary>
    public IReadOnlyList<GlobalExampleRegistration> GlobalExamples => _examples;

    /// <summary>
    /// Registers a global example provider for the given status code. Applies to every action
    /// that returns this status unless overridden by a more specific
    /// <see cref="ResponseExampleAttribute"/> on the method or controller.
    /// </summary>
    /// <typeparam name="TProvider">A type implementing <see cref="IExampleProvider{T}"/>.</typeparam>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="name">Optional name when registering multiple examples for the same status.</param>
    /// <param name="summary">Optional short summary shown in the OpenAPI UI.</param>
    /// <param name="description">Optional longer description.</param>
    /// <returns>The same instance for fluent chaining.</returns>
    public VianaResultOptions AddExample<TProvider>(int statusCode, string? name = null, string? summary = null, string? description = null)
        where TProvider : class
    {
        _examples.Add(new GlobalExampleRegistration(statusCode, typeof(TProvider), name, summary, description));
        return this;
    }
}

/// <summary>A globally-registered example provider entry.</summary>
public sealed record GlobalExampleRegistration(int StatusCode, Type ProviderType, string? Name = null, string? Summary = null, string? Description = null);
