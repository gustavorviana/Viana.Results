using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.Mediators
{
    public class Mediator : IMediator
    {
#if NET8_0_OR_GREATER
        private static readonly ActivitySource _activitySource = new("Financial.Management.Mediator", "1.0.0");
#endif
        private readonly ConcurrentDictionary<Type, HandlerInfo> _handleCache = new();
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

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

                if (result.Error != null)
                    activity?.SetTag("result.error", result.Error.Message);

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