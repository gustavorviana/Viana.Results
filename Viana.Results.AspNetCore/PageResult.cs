using System.Dynamic;
using System.Net;

namespace Viana.Results.AspNetCore
{
    /// <summary>
    /// Represents a paginated action result that returns a collection of items with pagination metadata.
    /// </summary>
    public class PageResult : ItemsResult
    {
        /// <summary>
        /// Gets or sets the total number of items across all pages.
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int Pages { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageResult"/> class with HTTP 200 OK status.
        /// </summary>
        public PageResult() : base(HttpStatusCode.OK)
        {
        }

        protected override object GetReturnObject(ResponseFormatOptions options)
        {
            return new PaginationResult
            {
                Data = Items ?? [],
                TotalItems = TotalItems,
                Pages = Pages,
                Error = Error?.GetResponse()
            };
        }

        private class PaginationResult : DataResponse
        {
            public long TotalItems { get; set; }
            public int Pages { get; set; }
        }
    }
}
