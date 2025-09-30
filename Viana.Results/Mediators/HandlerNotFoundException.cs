using System;

namespace Viana.Results.Mediators
{
    public class HandlerNotFoundException : Exception
    {
        public HandlerNotFoundException(Type requestType) : base($"No handler registered for request type {requestType.Name}")
        {

        }
    }
}
