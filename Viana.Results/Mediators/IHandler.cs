using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.Mediators
{
    /// <summary>
    /// Defines a handler for a specific request type that produces a result.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to handle.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
    public interface IHandler<TRequest, TResult>
        where TResult : IResult
        where TRequest : IRequest<TResult>
    {
        /// <summary>
        /// Handles the specified request and returns a result.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the handler result.</returns>
        Task<TResult> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}