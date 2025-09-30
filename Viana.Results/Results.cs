using System;
using System.Net;

namespace Viana.Results
{
    public static class Results
    {
        public static Result Success(string message)
        {
            return new Result(message);
        }

        public static Result Success(object data)
        {
            return new Result(data);
        }

        public static Result Success(object data, string message)
        {
            return new Result(data, message);
        }

        public static Result Success()
        {
            return new Result();
        }

        public static Result Failure(string message, HttpStatusCode statusCode = (HttpStatusCode)422)
        {
            return new Result(new ResultError(message), null, statusCode);
        }

        public static Result Failure(Exception exception, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            return new Result(new ExceptionError(exception), statusCode);
        }
    }
}
