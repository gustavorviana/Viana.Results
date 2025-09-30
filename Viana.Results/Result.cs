using System.Net;

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
        /// Gets the result message.
        /// </summary>
        public string Message { get; }

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
        public Result(TValue data, string message = null, HttpStatusCode status = HttpStatusCode.OK) : this(message, status)
        {
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{TValue}"/> class with a message.
        /// </summary>
        /// <param name="message">The result message.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(string message, HttpStatusCode status = HttpStatusCode.OK) : this(status)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{TValue}"/> class with an error.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="message">The error message.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(ResultError error, string message, HttpStatusCode status = HttpStatusCode.InternalServerError) : this(status)
        {
            Error = error;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{TValue}"/> class with an error.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(ResultError error, HttpStatusCode status = HttpStatusCode.InternalServerError) : this(status)
        {
            Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        public Result(HttpStatusCode status = HttpStatusCode.OK)
        {
            StatusCode = status;
        }

        public static implicit operator Result<TValue>(Result result)
        {
            if (result.Error != null)
                return new Result<TValue>(result.Error, result.Message, result.StatusCode);

            if (result.Data != null)
                return new Result<TValue>((TValue)result.Data, result.Message, result.StatusCode);

            return new Result<TValue>(result.Message, result.StatusCode);
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
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the result message.
        /// </summary>
        public string Message { get; } = string.Empty;

        /// <summary>
        /// Gets the result data.
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Gets the error information, if any.
        /// </summary>
        public ResultError Error { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class with a message.
        /// </summary>
        /// <param name="message">The result message.</param>
        /// <param name="data">The result data.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(string message, object data, HttpStatusCode status = HttpStatusCode.OK) : this(status)
        {
            Message = message;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class with an error.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="message">The error message.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(ResultError error, string message, HttpStatusCode status = HttpStatusCode.InternalServerError) : this(message, status)
        {
            Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class with an error.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="message">The error message.</param>
        /// <param name="data">Optional data related to the error.</param>
        /// <param name="status">The HTTP status code.</param>
        public Result(ResultError error, string message, object data, HttpStatusCode status = HttpStatusCode.InternalServerError) : this(message, status)
        {
            Error = error;
            Data = data;
        }

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
        public Result(string message, HttpStatusCode status = HttpStatusCode.OK) : this(status)
        {
            Message = message ?? string.Empty;
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
