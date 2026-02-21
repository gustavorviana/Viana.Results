namespace Viana.Results;

/// <summary>
/// Provides a non-generic data accessor for result instances.
/// </summary>
/// <remarks>
/// This interface exists purely for interoperability between generic (<c>Result&lt;T&gt;</c>)
/// and non-generic (<c>Result</c>) result types. It allows consumers to access the payload
/// in a uniform way when the concrete generic type is not known at compile time.
///
/// Implementations typically forward this property to the strongly-typed
/// <c>Result&lt;T&gt;.Data</c> member.
/// </remarks>
public interface IResultData
{
    /// <summary>
    /// Gets the underlying payload when available; otherwise <see langword="null" />.
    /// </summary>
    object? Data { get; }
}

public interface IResult
{
    /// <summary>
    /// Gets the HTTP status code associated with this result.
    /// </summary>
    /// <remarks>
    /// This value is typically aligned with common HTTP semantics (e.g., 200 for success, 400 for validation errors,
    /// 404 for not found, 500 for server errors), even when used outside an actual HTTP pipeline.
    /// </remarks>
    int Status { get; }

    /// <summary>
    /// Gets the problem details when the result represents an error; otherwise <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// When <see cref="Problem"/> is not <see langword="null" />, the result should be treated as unsuccessful and
    /// callers should prefer using <see cref="Problem"/> instead of any payload returned by typed results.
    /// </remarks>
    ProblemResult? Problem { get; }
}

/// <summary>
/// Represents a typed result that may contain a payload on success.
/// </summary>
/// <typeparam name="T">The payload type.</typeparam>
public interface IResult<out T> : IResult
{
    /// <summary>
    /// Gets the payload for successful results; otherwise <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// When <see cref="IResult.Problem"/> is not <see langword="null" />, <see cref="Data"/> should generally be
    /// considered undefined and typically remains <see langword="null" />.
    /// </remarks>
    T? Data { get; }
}
