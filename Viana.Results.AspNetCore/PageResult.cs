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

        protected override object GetReturnObject()
        {
            dynamic obj = new ExpandoObject();

            obj.TotalItems = TotalItems;
            obj.Pages = Pages;

            if (!string.IsNullOrEmpty(Message))
                obj.Message = Message;

            if (Error != null)
                obj.Error = Error.GetResponse();

            obj.Items = Items ?? [];

            return obj;
        }
    }
}
