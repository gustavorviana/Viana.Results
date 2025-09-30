using System;

namespace Viana.Results
{
    public class ExceptionError : ResultError
    {
        public Exception Exception { get; }

        public ExceptionError(Exception exception, string message) : base(message)
        {
            Exception = exception;
        }

        public ExceptionError(Exception exception) : this(exception, exception.Message)
        {
        }
    }
}
