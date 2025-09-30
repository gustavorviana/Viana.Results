using System.Collections.Generic;
using System.Linq;

namespace Viana.Results
{
    /// <summary>
    /// Represents a paginated result containing a collection of items with pagination metadata.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the collection.</typeparam>
    public class PaginatedResult<TValue> : Result<ICollection<TValue>>, IPaginatedResult
    {
        /// <summary>
        /// Gets or sets the total count of items across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int Pages { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedResult{TValue}"/> class with items and pagination data.
        /// </summary>
        /// <param name="items">The collection of items for the current page.</param>
        /// <param name="totalCount">The total count of items.</param>
        /// <param name="pages">The total number of pages.</param>
        public PaginatedResult(ICollection<TValue> items, int totalCount, int pages) : base(items)
        {
            TotalCount = totalCount;
            Pages = pages;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedResult{TValue}"/> class with an error.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="message">The error message.</param>
        public PaginatedResult(ResultError error, string message = null) : base(error, message)
        {
        }

        /// <summary>
        /// Converts the data collection to a queryable.
        /// </summary>
        /// <returns>A queryable collection of items.</returns>
        public IQueryable<TValue> AsQueryable()
        {
            return Data?.AsQueryable() ?? Enumerable.Empty<TValue>().AsQueryable();
        }
    }
}
