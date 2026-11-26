using System;

namespace Viana.Results.OpenApi;

/// <summary>
/// Endpoint metadata describing one documented response of a minimal API endpoint, added by
/// <see cref="VianaRouteHandlerBuilderExtensions"/>. Read by both OpenAPI pipelines.
/// </summary>
public sealed class VianaResponseMetadata
{
    /// <summary>Explicit status; when an example result is given, the status comes from it (and must match).</summary>
    public int? Status { get; init; }

    /// <summary>Documented success data type (<c>ProducesResult&lt;T&gt;</c> with a data type).</summary>
    public Type? DataType { get; init; }

    /// <summary>Example provider class (<see cref="IExampleProvider{T}"/>).</summary>
    public Type? ExampleProviderType { get; init; }

    /// <summary>Inline example: a Viana result instance.</summary>
    public IResult? InlineExample { get; init; }

    /// <summary>Problem title (<c>ProducesProblemResult(status, ...)</c>).</summary>
    public string? Title { get; init; }

    /// <summary>Problem type URI.</summary>
    public string? Type { get; init; }

    /// <summary>Problem detail.</summary>
    public string? Detail { get; init; }

    /// <summary>Declared as a problem response.</summary>
    public bool IsProblem { get; init; }

    /// <summary>Example name (several examples for one status).</summary>
    public string? Name { get; init; }

    /// <summary>Example summary.</summary>
    public string? Summary { get; init; }
}
