using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.Mediators
{
    public interface IHandler<TRequest, TResult>
        where TResult : IResult
        where TRequest : IRequest<TResult>
    {
        Task<TResult> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}