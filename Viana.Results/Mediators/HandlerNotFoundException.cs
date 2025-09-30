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
        public HandlerNotFoundException(Type requestType) : base($"No handler registered for request type {requestType.Name}")
        {

        }
    }
}
