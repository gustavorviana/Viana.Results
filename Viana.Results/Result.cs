using System.Net;
#if NET5_0_OR_GREATER
using System.Text.Json.Serialization;
#endif

namespace Viana.Results
{
    /// <summary>
    /// Represents a strongly-typed result with status, message, data, and error information.
    /// </summary>
    /// <typeparam name="TValue">The type of the result data.</typeparam>
    public class Result<TValue> : IResult<TValue>
    {
        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the result data.
        /// </summary>
        public TValue Data { get; }

        /// <summary>
        /// Gets the error information, if any.
        /// </summary>
        public ResultError Error { get; }

        object IResult.Data => Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{TValue}"/> class with data.
        /// </summary>
        /// <param name="data">The result data.</param>
        /// <param name="message">The optional message.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(TValue data, HttpStatusCode status = HttpStatusCode.OK)
        {
            Data = data;
            StatusCode = status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{TValue}"/> class with an error.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(ResultError error, HttpStatusCode status = HttpStatusCode.InternalServerError)
        {
            Error = error;
            StatusCode = status;
        }

        public static implicit operator Result<TValue>(Result result)
        {
            if (result.Error != null)
                return new Result<TValue>(result.Error, result.StatusCode);

            return new Result<TValue>((TValue)result.Data, result.StatusCode);
        }

        public static implicit operator Result<TValue>(TValue value)
        {
            return new Result<TValue>(value);
        }
    }

    /// <summary>
    /// Represents a non-generic result with status, message, data, and error information.
    /// </summary>
    public class Result : IResult
    {
        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
#if NET5_0_OR_GREATER
        [JsonIgnore]
#endif
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the result data.
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Gets the error information, if any.
        /// </summary>
        public ResultError Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class with an error.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(ResultError error, HttpStatusCode status = HttpStatusCode.InternalServerError) : this(status)
        {
            Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class with a message.
        /// </summary>
        /// <param name="message">The result message.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(object data, HttpStatusCode status = HttpStatusCode.OK) : this(status)
        {
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        public Result(HttpStatusCode status = HttpStatusCode.OK)
        {
            StatusCode = status;
        }
    }
}
