using System.Collections.Generic;
using System.Net;

namespace Viana.Results
{
    /// <summary>
    /// Represents a result containing a collection of items.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the collection.</typeparam>
    public class CollectionResult<TValue> : Result<ICollection<TValue>>, ICollectionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionResult{TValue}"/> class.
        /// </summary>
        /// <param name="data">The collection of items.</param>
        /// <param name="message">The optional message.</param>
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
}
