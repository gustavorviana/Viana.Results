using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.Mediators
{
    public interface IMediator
    {
        Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default) where TResult : IResult;
    }
}