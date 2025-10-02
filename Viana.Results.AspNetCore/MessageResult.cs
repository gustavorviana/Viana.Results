using System.Net;

namespace Viana.Results.AspNetCore
{
    /// <summary>
    /// Represents an action result that returns a message.
    /// </summary>
    public class MessageResult : ActionResultBase
    {
        /// <summary>
        /// Gets or sets the message to return.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageResult"/> class with HTTP 200 OK status.
        /// </summary>
        public MessageResult() : base(HttpStatusCode.OK)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageResult"/> class with a specified status code.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        public MessageResult(HttpStatusCode status) : base(status)
        {
        }

        /// <summary>
        /// Creates a successful message result with HTTP 200 OK status.
        /// </summary>
        /// <param name="message">The message to return. Defaults to "OK".</param>
        /// <returns>A message result with the specified message.</returns>
        public static MessageResult Ok(string message = "OK")
        {
            return new MessageResult { Message = message };
        }

        protected override object GetReturnObject()
        {
            return new DataResponse
            {
                Message = Message,
                Error = Error?.GetResponse()
            };
        }

        /// <summary>
        /// Creates a service unavailable result with HTTP 503 status and Retry-After header.
        /// </summary>
        /// <param name="retryAfter">The number of seconds to wait before retrying. Defaults to 120.</param>
        /// <returns>A service unavailable message result.</returns>
        public static MessageResult ServiceUnavailable(int retryAfter = 120)
        {
            MessageResult result = new(HttpStatusCode.ServiceUnavailable);

            result.Headers.Add("Retry-After", retryAfter.ToString());
            return result;
        }
    }
}
