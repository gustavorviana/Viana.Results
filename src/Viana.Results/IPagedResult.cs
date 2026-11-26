using System.Collections.Generic;

namespace Viana.Results;

/// <summary>
/// Result that contains a paginated list of items. It is an envelope: the body keeps the items and the paging
/// metadata (<c>{ data, pageNumber, totalPages }</c>).
/// </summary>
/// <typeparam name="T">Type of each item in the page.</typeparam>
public interface IPagedResult<out T> : IListResult<T>, IEnvelopeResult<IReadOnlyList<T>>
{
    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    int PageNumber { get; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    int TotalPages { get; }
}
