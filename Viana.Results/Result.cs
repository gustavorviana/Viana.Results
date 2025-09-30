using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Viana.Results
{
    public class PaginatedResult<TValue> : Result<ICollection<TValue>>, IPaginatedResult
    {
        public int TotalCount { get; set; }
        public int Pages { get; set; }

        public PaginatedResult(ICollection<TValue> items, int totalCount, int pages) : base(items)
        {
            TotalCount = totalCount;
            Pages = pages;
        }

        public PaginatedResult(ResultError error, string message = null) : base(error, message)
        {
        }

        public IQueryable<TValue> AsQueryable()
        {
            return Data?.AsQueryable() ?? Enumerable.Empty<TValue>().AsQueryable();
        }
    }

    public class CollectionResult<TValue> : Result<ICollection<TValue>>, ICollectionResult
    {
        public CollectionResult(ICollection<TValue> data, string message = null) : base(data, message)
        {
        }

        private CollectionResult(string message, HttpStatusCode status = HttpStatusCode.OK) : base(message, status)
        {
        }

        private CollectionResult(ResultError error, string message, HttpStatusCode status = HttpStatusCode.InternalServerError) : base(error, message, status)
        {
        }

        public static implicit operator CollectionResult<TValue>(Result result)
        {
            if (result.Error != null)
                return new CollectionResult<TValue>(result.Error, result.Message, result.StatusCode);

            if (result.Data != null)
                return new CollectionResult<TValue>((ICollection<TValue>)result.Data, result.Message);

            return new CollectionResult<TValue>(result.Message, result.StatusCode);
        }

        public static implicit operator CollectionResult<TValue>(List<TValue> value)
        {
            return new CollectionResult<TValue>(value);
        }

        public static implicit operator CollectionResult<TValue>(TValue[] value)
        {
            return new CollectionResult<TValue>(value);
        }

        public static implicit operator Result(CollectionResult<TValue> value)
        {
            return new Result(value.Data, value.Message, value.StatusCode);
        }
    }

    public class Result<TValue> : IResult<TValue>
    {
        public HttpStatusCode StatusCode { get; }
        public string Message { get; }

        public TValue Data { get; }

        public ResultError Error { get; }

        object IResult.Data => Data;

        public Result(TValue data, string message = null, HttpStatusCode status = HttpStatusCode.OK) : this(message, status)
        {
            Data = data;
        }

        public Result(string message, HttpStatusCode status = HttpStatusCode.OK) : this(status)
        {
            Message = message;
        }

        public Result(ResultError error, string message, HttpStatusCode status = HttpStatusCode.InternalServerError) : this(status)
        {
            Error = error;
            Message = message ?? string.Empty;
        }

        public Result(ResultError error, HttpStatusCode status = HttpStatusCode.InternalServerError) : this(status)
        {
            Error = error;
        }

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

    public class Result : IResult
    {
        public HttpStatusCode StatusCode { get; }
        public string Message { get; } = string.Empty;

        public object Data { get; }

        public ResultError Error { get; }

        public Result(object data, string message = null, HttpStatusCode status = HttpStatusCode.OK) : this(message, status)
        {
            Data = data;
        }

        public Result(string message, HttpStatusCode status = HttpStatusCode.OK) : this(status)
        {
            Message = message;
        }

        public Result(ResultError error, string message, HttpStatusCode status = HttpStatusCode.InternalServerError) : this(status)
        {
            Error = error;
            Message = message ?? string.Empty;
        }

        public Result(ResultError error, HttpStatusCode status = HttpStatusCode.InternalServerError) : this(status)
        {
            Error = error;
        }

        public Result(HttpStatusCode status = HttpStatusCode.OK)
        {
            StatusCode = status;
        }
    }
}
