using System.Collections.Generic;
using System.Net;

namespace Viana.Results.AspNetCore
{
    /// <summary>
    /// Represents an action result that returns a collection of items.
    /// </summary>
    public class ItemsResult : ActionResultBase
    {
        /// <summary>
        /// Gets or sets the collection of items to return.
        /// </summary>
        public ICollection<object> Items { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsResult"/> class with HTTP 200 OK status.
        /// </summary>
        public ItemsResult() : base(HttpStatusCode.OK)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsResult"/> class with a specified status code.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        public ItemsResult(HttpStatusCode status) : base(status)
        {
        }

        protected override object GetReturnObject(ResponseFormatOptions options)
        {
            Items ??= [];

            return new DataResponse
            {
                Data = Items
            };
        }
    }
}
