using System.Net;

namespace Viana.Results
{
    public interface IPaginatedResult : ICollectionResult
    {
        public int TotalCount { get; }
        public int Pages { get; }
    }

    public interface ICollectionResult
    {
    }

    public interface IResult<TValue> : IResult
    {
        new TValue Data { get; }
    }

    public interface IResult
    {
        HttpStatusCode StatusCode { get; }
        string Message { get; }
        object Data { get; }
        ResultError Error { get; }
    }
}
