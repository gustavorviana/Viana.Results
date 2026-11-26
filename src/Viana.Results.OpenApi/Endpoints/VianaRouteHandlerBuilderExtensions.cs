using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Viana.Results.OpenApi;

/// <summary>
/// Documents Viana responses on minimal API endpoints (read by both OpenAPI pipelines). Overloads exist for
/// <see cref="RouteHandlerBuilder"/>, <see cref="RouteGroupBuilder"/> and any <see cref="IEndpointConventionBuilder"/>.
/// </summary>
public static class VianaRouteHandlerBuilderExtensions
{
    /// <summary>
    /// Documents a success response. <typeparamref name="T"/> is either the data type (status 200 unless
    /// <paramref name="status"/> is given) or an example provider class (<see cref="IExampleProvider{T}"/>,
    /// auto-detected) whose result defines status and example (an explicit status must match it).
    /// </summary>
    public static RouteHandlerBuilder ProducesResult<T>(this RouteHandlerBuilder builder, int? status = null, string? name = null, string? summary = null)
        => Add(builder, IsProvider(typeof(T))
            ? new VianaResponseMetadata { ExampleProviderType = typeof(T), Status = status, Name = name, Summary = summary }
            : new VianaResponseMetadata { DataType = typeof(T), Status = status, Name = name, Summary = summary });

    /// <summary>Documents a response with an inline Viana result as example (status from the result).</summary>
    public static RouteHandlerBuilder ProducesResult(this RouteHandlerBuilder builder, IResult example, string? name = null, string? summary = null)
        => Add(builder, new VianaResponseMetadata { InlineExample = example ?? throw new ArgumentNullException(nameof(example)), Name = name, Summary = summary });

    /// <summary>Documents an RFC 9457 problem response with metadata.</summary>
    public static RouteHandlerBuilder ProducesProblemResult(this RouteHandlerBuilder builder, int status, string? title = null, string? type = null, string? detail = null)
        => Add(builder, new VianaResponseMetadata { IsProblem = true, Status = status, Title = title, Type = type, Detail = detail });

    /// <summary>Documents a problem response from an example provider class (status from its result).</summary>
    public static RouteHandlerBuilder ProducesProblemResult<TProvider>(this RouteHandlerBuilder builder, string? name = null, string? summary = null)
        => ProducesProblemResult(builder, typeof(TProvider), name, summary);

    /// <summary>Documents a problem response from an example provider type (status from its result).</summary>
    public static RouteHandlerBuilder ProducesProblemResult(this RouteHandlerBuilder builder, Type providerType, string? name = null, string? summary = null)
        => Add(builder, new VianaResponseMetadata { IsProblem = true, ExampleProviderType = providerType ?? throw new ArgumentNullException(nameof(providerType)), Name = name, Summary = summary });

    /// <summary>Documents a problem response with an inline failed Viana result (status from the result).</summary>
    public static RouteHandlerBuilder ProducesProblemResult(this RouteHandlerBuilder builder, IResult failedExample, string? name = null, string? summary = null)
        => Add(builder, new VianaResponseMetadata { IsProblem = true, InlineExample = failedExample ?? throw new ArgumentNullException(nameof(failedExample)), Name = name, Summary = summary });

    /// <summary>
    /// Documents a success response. <typeparamref name="T"/> is either the data type (status 200 unless
    /// <paramref name="status"/> is given) or an example provider class (<see cref="IExampleProvider{T}"/>,
    /// auto-detected) whose result defines status and example (an explicit status must match it).
    /// </summary>
    public static RouteGroupBuilder ProducesResult<T>(this RouteGroupBuilder builder, int? status = null, string? name = null, string? summary = null)
        => Add(builder, IsProvider(typeof(T))
            ? new VianaResponseMetadata { ExampleProviderType = typeof(T), Status = status, Name = name, Summary = summary }
            : new VianaResponseMetadata { DataType = typeof(T), Status = status, Name = name, Summary = summary });

    /// <summary>Documents a response with an inline Viana result as example (status from the result).</summary>
    public static RouteGroupBuilder ProducesResult(this RouteGroupBuilder builder, IResult example, string? name = null, string? summary = null)
        => Add(builder, new VianaResponseMetadata { InlineExample = example ?? throw new ArgumentNullException(nameof(example)), Name = name, Summary = summary });

    /// <summary>Documents an RFC 9457 problem response with metadata.</summary>
    public static RouteGroupBuilder ProducesProblemResult(this RouteGroupBuilder builder, int status, string? title = null, string? type = null, string? detail = null)
        => Add(builder, new VianaResponseMetadata { IsProblem = true, Status = status, Title = title, Type = type, Detail = detail });

    /// <summary>Documents a problem response from an example provider class (status from its result).</summary>
    public static RouteGroupBuilder ProducesProblemResult<TProvider>(this RouteGroupBuilder builder, string? name = null, string? summary = null)
        => ProducesProblemResult(builder, typeof(TProvider), name, summary);

    /// <summary>Documents a problem response from an example provider type (status from its result).</summary>
    public static RouteGroupBuilder ProducesProblemResult(this RouteGroupBuilder builder, Type providerType, string? name = null, string? summary = null)
        => Add(builder, new VianaResponseMetadata { IsProblem = true, ExampleProviderType = providerType ?? throw new ArgumentNullException(nameof(providerType)), Name = name, Summary = summary });

    /// <summary>Documents a problem response with an inline failed Viana result (status from the result).</summary>
    public static RouteGroupBuilder ProducesProblemResult(this RouteGroupBuilder builder, IResult failedExample, string? name = null, string? summary = null)
        => Add(builder, new VianaResponseMetadata { IsProblem = true, InlineExample = failedExample ?? throw new ArgumentNullException(nameof(failedExample)), Name = name, Summary = summary });

    /// <summary>
    /// Documents a success response. <typeparamref name="T"/> is either the data type (status 200 unless
    /// <paramref name="status"/> is given) or an example provider class (<see cref="IExampleProvider{T}"/>,
    /// auto-detected) whose result defines status and example (an explicit status must match it).
    /// </summary>
    public static IEndpointConventionBuilder ProducesResult<T>(this IEndpointConventionBuilder builder, int? status = null, string? name = null, string? summary = null)
        => Add(builder, IsProvider(typeof(T))
            ? new VianaResponseMetadata { ExampleProviderType = typeof(T), Status = status, Name = name, Summary = summary }
            : new VianaResponseMetadata { DataType = typeof(T), Status = status, Name = name, Summary = summary });

    /// <summary>Documents a response with an inline Viana result as example (status from the result).</summary>
    public static IEndpointConventionBuilder ProducesResult(this IEndpointConventionBuilder builder, IResult example, string? name = null, string? summary = null)
        => Add(builder, new VianaResponseMetadata { InlineExample = example ?? throw new ArgumentNullException(nameof(example)), Name = name, Summary = summary });

    /// <summary>Documents an RFC 9457 problem response with metadata.</summary>
    public static IEndpointConventionBuilder ProducesProblemResult(this IEndpointConventionBuilder builder, int status, string? title = null, string? type = null, string? detail = null)
        => Add(builder, new VianaResponseMetadata { IsProblem = true, Status = status, Title = title, Type = type, Detail = detail });

    /// <summary>Documents a problem response from an example provider class (status from its result).</summary>
    public static IEndpointConventionBuilder ProducesProblemResult<TProvider>(this IEndpointConventionBuilder builder, string? name = null, string? summary = null)
        => ProducesProblemResult(builder, typeof(TProvider), name, summary);

    /// <summary>Documents a problem response from an example provider type (status from its result).</summary>
    public static IEndpointConventionBuilder ProducesProblemResult(this IEndpointConventionBuilder builder, Type providerType, string? name = null, string? summary = null)
        => Add(builder, new VianaResponseMetadata { IsProblem = true, ExampleProviderType = providerType ?? throw new ArgumentNullException(nameof(providerType)), Name = name, Summary = summary });

    /// <summary>Documents a problem response with an inline failed Viana result (status from the result).</summary>
    public static IEndpointConventionBuilder ProducesProblemResult(this IEndpointConventionBuilder builder, IResult failedExample, string? name = null, string? summary = null)
        => Add(builder, new VianaResponseMetadata { IsProblem = true, InlineExample = failedExample ?? throw new ArgumentNullException(nameof(failedExample)), Name = name, Summary = summary });

    /// <summary>
    /// Last resort: removes the success response inferred from the return type, so only the responses
    /// registered by hand (<c>ProducesResult</c>, <c>Produces</c>) and the problem responses are documented.
    /// See <see cref="WithoutDefaultResultAttribute"/>. Documentation only.
    /// </summary>
    public static RouteHandlerBuilder WithoutDefaultResult(this RouteHandlerBuilder builder)
        => AddMarker(builder);

    /// <inheritdoc cref="WithoutDefaultResult(RouteHandlerBuilder)"/>
    public static RouteGroupBuilder WithoutDefaultResult(this RouteGroupBuilder builder)
        => AddMarker(builder);

    /// <inheritdoc cref="WithoutDefaultResult(RouteHandlerBuilder)"/>
    public static IEndpointConventionBuilder WithoutDefaultResult(this IEndpointConventionBuilder builder)
        => AddMarker(builder);

    private static TBuilder AddMarker<TBuilder>(TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        builder.Add(e => e.Metadata.Add(new WithoutDefaultResultAttribute()));
        return builder;
    }

    private static TBuilder Add<TBuilder>(TBuilder builder, VianaResponseMetadata metadata) where TBuilder : IEndpointConventionBuilder
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        builder.Add(e => e.Metadata.Add(metadata));
        return builder;
    }

    private static bool IsProvider(Type type)
        => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExampleProvider<>));
}
