using System;
using System.Collections.Generic;

namespace Viana.Results;

/// <summary>
/// Result type for operations that return a paginated list of items.
/// </summary>
/// <typeparam name="T">Type of each item in the page.</typeparam>
public class PagedResult<T>(IReadOnlyList<T> data, int pageNumber, int totalPages, ProblemResult? problem = null) : ListResult<T>(data, problem), IPagedResult<T>
{
    /// <inheritdoc />
    public int PageNumber => pageNumber;

    /// <inheritdoc />
    public int TotalPages => totalPages;

    /// <summary>
    /// Creates a <see cref="PagedResult{T}"/> by computing <see cref="TotalPages"/> from
    /// <paramref name="pageSize"/> and <paramref name="totalCount"/>.
    /// </summary>
    /// <param name="data">The items in the current page.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The page size used to compute the total page count. A value of zero produces zero pages.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <returns>A successful <see cref="PagedResult{T}"/> containing the items and computed page metadata.</returns>
    public static PagedResult<T> Create(IReadOnlyList<T> data, int pageNumber, int pageSize, int totalCount)
    {
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        return new PagedResult<T>(data, pageNumber, totalPages);
    }

    /// <summary>
    /// Implicitly converts a <see cref="ProblemResult"/> into a failure <see cref="PagedResult{T}"/>
    /// with an empty page and zero metadata.
    /// </summary>
    /// <param name="problem">The problem details describing the failure.</param>
    public static implicit operator PagedResult<T>(ProblemResult problem) => new([], 0, 0, problem);
}