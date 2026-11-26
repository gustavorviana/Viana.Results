using System;
using Viana.Results.Mediators.Dispatch;

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
            RequestType = requestType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class with the specified request type and inner exception.
        /// </summary>
        /// <param name="requestType">The type of the request for which no handler was found.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public HandlerNotFoundException(Type requestType, Exception innerException)
            : base($"No handler registered for request type {requestType.Name}", innerException)
        {
            RequestType = requestType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class for a request and result type.
        /// </summary>
        /// <param name="requestType">The type of the request for which no handler was found.</param>
        /// <param name="resultType">The result type the handler should return.</param>
        public HandlerNotFoundException(Type requestType, Type resultType)
            : base($"No handler registered for request type {TypeNames.Get(requestType)} returning {TypeNames.Get(resultType)}.")
        {
            RequestType = requestType;
            ResultType = resultType;
        }

        /// <summary>The request type without a registered handler.</summary>
        public Type RequestType { get; }

        /// <summary>The expected result type, when known.</summary>
        public Type? ResultType { get; }
    }
}
