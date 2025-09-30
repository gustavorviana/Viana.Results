using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.Mediators
{
    /// <summary>
    /// Implementation of the mediator pattern for dispatching requests to their corresponding handlers.
    /// </summary>
    public class Mediator : IMediator
    {
#if NET8_0_OR_GREATER
        private static readonly ActivitySource _activitySource = new("Financial.Management.Mediator", "1.0.0");
#endif
        private readonly ConcurrentDictionary<Type, HandlerInfo> _handleCache = new();
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve handler instances.</param>
        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Sends a request to the appropriate handler and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the handler result.</returns>
        /// <exception cref="HandlerNotFoundException">Thrown when no handler is registered for the request type.</exception>
        public async Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default) where TResult : IResult
        {
            var requestType = request.GetType();
            var resultType = typeof(TResult);

#if NET8_0_OR_GREATER
            using var activity = new MediatorActivity(_activitySource, requestType);
#else
            var activity = new MediatorActivity();
#endif
            activity?.SetTag("request.type", requestType.Name);
            activity?.SetTag("result.type", resultType.Name);

            try
            {
                var handlerInfo = GetInfo(requestType, resultType);
                var handler = _serviceProvider.GetService(handlerInfo.Type) ?? throw new HandlerNotFoundException(requestType);

                activity?.SetTag("handler.type", handler.GetType().Name);

                var result = await handlerInfo.InvokeAsync(handler, request, cancellationToken);

                activity?.SetTag("result.success", result.Error == null);
                activity?.SetTag("result.statusCode", (int)result.StatusCode);
                activity?.SetOk();

                return result;
            }
            catch (Exception ex)
            {
                if (ex is HandlerNotFoundException)
                    throw;

                activity?.SerError(ex);

                throw;
            }
        }

        private HandlerInfo GetInfo(Type requestType, Type resultType)
        {
            return _handleCache.GetOrAdd(requestType, _ =>
            {
                var type = typeof(IHandler<,>).MakeGenericType(requestType, resultType);
                return new HandlerInfo(type, type.GetMethod("Handle")!);
            });
        }

        private class HandlerInfo(Type type, MethodInfo handleMethod)
        {
            public Type Type => type;

            public async Task<TResult> InvokeAsync<TResult>(object handlerInstance, IRequest<TResult> request, CancellationToken cancellationToken) where TResult : IResult
            {
                try
                {
                    var resultTask = handleMethod.Invoke(handlerInstance, [request, cancellationToken]);
                    return await (Task<TResult>)resultTask!;
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.GetBaseException();
                }
            }
        }
    }
}