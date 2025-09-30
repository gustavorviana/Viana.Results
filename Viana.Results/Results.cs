using System;
using System.Collections.Generic;
using System.Net;

namespace Viana.Results
{
    /// <summary>
    /// Provides factory methods for creating result instances.
    /// </summary>
    public static class Results
    {
        /// <summary>
        /// Creates a successful result with a message.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <returns>A successful result.</returns>
        public static Result Success(string message)
        {
            return new Result(message);
        }

        /// <summary>
        /// Creates a successful result with data.
        /// </summary>
        /// <param name="data">The result data.</param>
        /// <returns>A successful result.</returns>
        public static Result Success(object data)
        {
            return new Result(data);
        }

        /// <summary>
        /// Creates a successful result with data and message.
        /// </summary>
        /// <param name="data">The result data.</param>
        /// <param name="message">The success message.</param>
        /// <returns>A successful result.</returns>
        public static Result Success(object data, string message)
        {
            return new Result(data, message);
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static Result Success()
        {
            return new Result();
        }

        /// <summary>
        /// Creates a failure result with a message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code. Defaults to 422 (Unprocessable Entity).</param>
        /// <returns>A failure result.</returns>
        public static Result Failure(string message, HttpStatusCode statusCode = (HttpStatusCode)422)
        {
            return new Result(new ResultError(message), null, statusCode);
        }

        /// <summary>
        /// Creates a failure result from an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="statusCode">The HTTP status code. Defaults to 500 (Internal Server Error).</param>
        /// <returns>A failure result.</returns>
        public static Result Failure(Exception exception, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            return new Result(new ExceptionError(exception), statusCode);
        }

        /// <summary>
        /// Creates a business rule violated result with a message.
        /// </summary>
        /// <param name="message">The business rule violation message.</param>
        /// <param name="data">Optional data associated with the violation.</param>
        /// <returns>A business rule violated result with HTTP 422 status.</returns>
        public static Result BusinessRuleViolated(string message, object data = null)
        {
            if (data == null)
                return new Result(new ResultError(message), message, (HttpStatusCode)422);

            return new Result(new DataResultError(data, message), message, (HttpStatusCode)422);
        }

        /// <summary>
        /// Creates a Result indicating that the requested resource was not found.
        /// </summary>
        /// <param name="message">The error message. Default: "The requested resource was not found."</param>
        /// <returns>A Result with NotFound status (HTTP 404).</returns>
        public static Result NotFound(string message = "The requested resource was not found.")
        {
            return new Result(new ResultError(message), message, HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Creates a Result indicating unauthorized access.
        /// </summary>
        /// <param name="message">The error message. Default: "Unauthorized access."</param>
        /// <returns>A Result with Unauthorized status (HTTP 401).</returns>
        public static Result Unauthorized(string message = "Unauthorized access.")
        {
            return new Result(new ResultError(message), message, HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Creates a Result indicating forbidden access.
        /// </summary>
        /// <param name="message">The error message. Default: "Forbidden access."</param>
        /// <returns>A Result with Forbidden status (HTTP 403).</returns>
        public static Result Forbidden(string message = "Forbidden access.")
        {
            return new Result(new ResultError(message), message, HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// Creates a Result indicating a conflict occurred.
        /// </summary>
        /// <param name="message">The error message. Default: "Conflict occurred."</param>
        /// <returns>A Result with Conflict status (HTTP 409).</returns>
        public static Result Conflict(string message = "Conflict occurred.")
        {
            return new Result(new ResultError(message), message, HttpStatusCode.Conflict);
        }

        /// <summary>
        /// Creates a Result indicating validation failure with detailed error information.
        /// </summary>
        /// <param name="errors">Dictionary containing field names as keys and arrays of validation error messages as values.</param>
        /// <param name="message">The general validation error message. Default: "Validation failed"</param>
        /// <returns>A Result with Conflict status (HTTP 409) containing validation errors.</returns>
        public static Result Validation(Dictionary<string, string[]> errors, string message = "Validation failed")
        {
            return new Result(new ValidationError(errors, message), HttpStatusCode.Conflict);
        }
    }
}
