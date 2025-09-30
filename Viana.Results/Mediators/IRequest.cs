namespace Viana.Results.Mediators
{
    /// <summary>
    /// Marker interface for requests that return a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the request handler.</typeparam>
    public interface IRequest<TResult> where TResult : IResult
    {
    }
}