using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Viana.Results
{
    /// <summary>
    /// Represents a paginated result containing a collection of items with pagination metadata.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the collection.</typeparam>
    public class PaginatedResult<TValue> : IResult<IReadOnlyList<TValue>>, IPaginatedResult
    {
        private List<object> entities;

        /// <summary>
        /// Gets or sets the total count of items across all pages.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int Pages { get; set; }

        public IReadOnlyList<TValue> Data { get; }

        public HttpStatusCode StatusCode { get; } = HttpStatusCode.OK;

        object IResult.Data => Data;

        public ResultError Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{TValue}"/> class with data.
        /// </summary>
        /// <param name="data">The result data.</param>
        /// <param name="status">The HTTP status code.</param>
        public PaginatedResult(IReadOnlyList<TValue> data, HttpStatusCode status = HttpStatusCode.OK)
        {
            Data = data;
            StatusCode = status;
            Total = data.Count;
            Pages = Total > 0 ? 1 : 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedResult{TValue}"/> class with items and pagination data.
        /// </summary>
        /// <param name="items">The collection of items for the current page.</param>
        /// <param name="totalCount">The total count of items.</param>
        /// <param name="pages">The total number of pages.</param>
        public PaginatedResult(IReadOnlyList<TValue> items, int totalCount, int pages)
        {
            Total = totalCount;
            Pages = pages;
            Data = items;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedResult{TValue}"/> class with an error.
        /// </summary>
        /// <param name="error">The error information.</param>
        public PaginatedResult(ResultError error, HttpStatusCode status = HttpStatusCode.InternalServerError)
        {
            Error = error;
            StatusCode = status;
        }

        public PaginatedResult(List<object> entities)
        {
            this.entities = entities;
        }

        /// <summary>
        /// Converts the data collection to a queryable.
        /// </summary>
        /// <returns>A queryable collection of items.</returns>
        public IQueryable<TValue> AsQueryable()
        {
            return Data?.AsQueryable() ?? Enumerable.Empty<TValue>().AsQueryable();
        }

        public static implicit operator PaginatedResult<TValue>(Result result)
        {
            if (result.Error != null)
                return new PaginatedResult<TValue>(result.Error, result.StatusCode);

            return new PaginatedResult<TValue>((IReadOnlyList<TValue>)result.Data, result.StatusCode);
        }
    }
}
