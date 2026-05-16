using System.Collections.Generic;

namespace Viana.Results;

/// <summary>
/// Result type for operations that return a list of items.
/// </summary>
/// <typeparam name="TValue">Type of each item in the list.</typeparam>
public class ListResult<TValue> : Result, IListResult<TValue>
{
    /// <inheritdoc />
    public IReadOnlyList<TValue> Data { get; }

    /// <summary>
    /// Initializes a new <see cref="ListResult{TValue}"/> with the given items, optionally representing a failure.
    /// </summary>
    /// <param name="data">The list of items. May be empty.</param>
    /// <param name="problem">The problem details when the result represents a failure; <see langword="null"/> on success (status 200).</param>
    public ListResult(IReadOnlyList<TValue> data, ProblemResult? problem = null)
        : base(problem?.Status ?? 200, data, problem)
    {
        Data = data;
    }

    /// <summary>
    /// Implicitly converts a <see cref="ProblemResult"/> into a failure <see cref="ListResult{TValue}"/>
    /// with an empty payload and the problem's status code.
    /// </summary>
    /// <param name="problem">The problem details describing the failure.</param>
    public static implicit operator ListResult<TValue>(ProblemResult problem) => new([], problem);

    /// <summary>
    /// Implicitly converts a <see cref="List{T}"/> into a successful <see cref="ListResult{TValue}"/> with status 200.
    /// </summary>
    /// <param name="items">The items to wrap.</param>
    public static implicit operator ListResult<TValue>(List<TValue> items) => new(items);
}
