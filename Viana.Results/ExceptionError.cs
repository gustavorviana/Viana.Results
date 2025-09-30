using System;

namespace Viana.Results
{
    /// <summary>
    /// Represents an error that wraps an exception.
    /// </summary>
    public class ExceptionError : ResultError
    {
        /// <summary>
        /// Gets the exception associated with this error.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionError"/> class with the specified exception and message.
        /// </summary>
        /// <param name="exception">The exception that caused the error.</param>
        /// <param name="message">The error message.</param>
        public ExceptionError(Exception exception, string message) : base(message)
        {
            Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionError"/> class with the specified exception.
        /// The error message is taken from the exception's message.
        /// </summary>
        /// <param name="exception">The exception that caused the error.</param>
        public ExceptionError(Exception exception) : this(exception, exception.Message)
        {
        }
    }
}
