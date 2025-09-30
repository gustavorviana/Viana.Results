using System.Net;

namespace Viana.Results
{
    /// <summary>
    /// Represents a paginated result with total count and page information.
    /// </summary>
    public interface IPaginatedResult : ICollectionResult
    {
        /// <summary>
        /// Gets the total count of items across all pages.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        public int Pages { get; }
    }

    /// <summary>
    /// Marker interface for collection-based results.
    /// </summary>
    public interface ICollectionResult
    {
    }

    /// <summary>
    /// Represents a strongly-typed result.
    /// </summary>
    /// <typeparam name="TValue">The type of the result data.</typeparam>
    public interface IResult<TValue> : IResult
    {
        /// <summary>
        /// Gets the result data.
        /// </summary>
        new TValue Data { get; }
    }

    /// <summary>
    /// Represents a result with status code, message, data, and error information.
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the result message.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets the result data.
        /// </summary>
        object Data { get; }

        /// <summary>
        /// Gets the error information, if any.
        /// </summary>
        ResultError Error { get; }
    }
}
