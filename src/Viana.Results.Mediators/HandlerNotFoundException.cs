using System;

namespace Viana.Results.Mediators
{
    /// <summary>
    /// Exception thrown when no handler is found for a specific request type.
    /// </summary>
    public class HandlerNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class with the specified request type.
        /// </summary>
        /// <param name="requestType">The type of the request for which no handler was found.</param>
        public HandlerNotFoundException(Type requestType)
            : base($"No handler registered for request type {requestType.Name}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class with the specified request type and inner exception.
        /// </summary>
        /// <param name="requestType">The type of the request for which no handler was found.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public HandlerNotFoundException(Type requestType, Exception innerException)
            : base($"No handler registered for request type {requestType.Name}", innerException)
        {
        }
    }
}
