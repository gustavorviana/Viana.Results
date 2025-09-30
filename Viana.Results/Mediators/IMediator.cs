using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.Mediators
{
    /// <summary>
    /// Defines a mediator for dispatching requests to their corresponding handlers.
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Sends a request to the appropriate handler and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the handler result.</returns>
        Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default) where TResult : IResult;
    }
}