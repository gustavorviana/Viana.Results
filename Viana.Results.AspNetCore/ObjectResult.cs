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
        public object Data { get; set; }

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
                Data = obj,
                Error = new DataResultError(obj)
            };
        }

        protected override object GetReturnObject()
        {
            if (Error != null)
                return Error.GetResponse();

            if (Data == null)
                return null;

            var type = Data.GetType();

            if (type.IsPrimitive || type.IsValueType || type == typeof(string))
                return new DataResponse { Data = Data };

            return Data;
        }
    }
}
