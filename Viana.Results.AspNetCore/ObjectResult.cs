using System.Dynamic;
using System.Net;

namespace Viana.Results.AspNetCore
{
    /// <summary>
    /// Represents an action result that returns an object with an optional message.
    /// </summary>
    public class ObjectResult : ActionResultBase
    {
        /// <summary>
        /// Gets or sets the result object to return.
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Gets or sets the message to return.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectResult"/> class with HTTP 200 OK status.
        /// </summary>
        public ObjectResult() : base(HttpStatusCode.OK)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectResult"/> class with a specified status code.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        public ObjectResult(HttpStatusCode status) : base(status)
        {
        }

        /// <summary>
        /// Creates an error object result with the specified status code and error data.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="obj">The error object.</param>
        /// <returns>An object result representing an error.</returns>
        public static ObjectResult ForError(HttpStatusCode status, object obj)
        {
            return new ObjectResult(status)
            {
                Result = obj,
                Error = new DataResultError(obj)
            };
        }

        protected override object GetReturnObject()
        {
            dynamic obj = new ExpandoObject();
            if (Error != null)
                obj.Error = Error.GetResponse();

            obj.Result = Result;

            if (!string.IsNullOrEmpty(Message))
                obj.Message = Message;

            return obj;
        }
    }
}
