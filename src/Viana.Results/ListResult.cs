using System.Collections.Generic;

namespace Viana.Results;

/// <summary>
/// Result type for operations that return a list of items.
/// </summary>
/// <typeparam name="TValue">Type of each item in the list.</typeparam>
public class ListResult<TValue> : Result, IListResult<TValue>
{
    public IReadOnlyList<TValue> Data { get; }

    public ListResult(IReadOnlyList<TValue> data, ProblemResult? problem = null)
        : base(problem?.Status ?? 200, data, problem)
    {
        Data = data;
    }

    public static implicit operator ListResult<TValue>(ProblemResult problem) => new([], problem);

    public static implicit operator ListResult<TValue>(List<TValue> items) => new(items);
}
