using System.Collections.Generic;
using System.Net;

namespace Viana.Results
{
    /// <summary>
    /// Represents a result containing a collection of items.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the collection.</typeparam>
    public class CollectionResult<TValue> : IResult<ICollection<TValue>>, ICollectionResult
    {
        public ICollection<TValue> Data { get; }

        public HttpStatusCode StatusCode { get; } = HttpStatusCode.OK;

        object IResult.Data => Data;

        public ResultError Error { get; }

        private CollectionResult(ICollection<TValue> data, HttpStatusCode status = HttpStatusCode.OK) : this(data)
        {
            StatusCode = status;
        }

        private CollectionResult(ResultError error, HttpStatusCode status = HttpStatusCode.InternalServerError)
        {
            Data = [];
            Error = error;
            StatusCode = status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionResult{TValue}"/> class.
        /// </summary>
        /// <param name="data">The collection of items.</param>
        /// <param name="message">The optional message.</param>
        public CollectionResult(ICollection<TValue> data)
        {
            Data = data ?? [];
        }

        public static implicit operator CollectionResult<TValue>(Result result)
        {
            if (result.Error != null)
                return new CollectionResult<TValue>(result.Error, result.StatusCode);

            return new CollectionResult<TValue>((ICollection<TValue>)result.Data, result.StatusCode);
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
            if (value.Error != null)
                return new Result(value.Error, value.StatusCode);

            return new Result(value.Data, value.StatusCode);
        }
    }
}
