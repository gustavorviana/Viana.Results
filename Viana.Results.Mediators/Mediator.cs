using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.Mediators
{
    /// <summary>
    /// Implementation of the mediator pattern for dispatching requests to their corresponding handlers.
    /// </summary>
    public class Mediator : IMediator
    {
#if NET5_0_OR_GREATER
        private static readonly ActivitySource _activitySource;

        static Mediator()
        {
            var asm = typeof(Mediator).GetTypeInfo().Assembly;

            // Preferred: informational version
            var version =
                asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? asm.GetName().Version?.ToString()
                ?? "1.0.0";

            _activitySource = new ActivitySource(
                asm.GetName().Name ?? "Viana.Results.Mediator",
                version
            );
        }
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
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            var requestType = request.GetType();
            var resultType = typeof(TResult);

#if NET5_0_OR_GREATER
            using var activity = new MediatorActivity(_activitySource, requestType);
#else
            var activity = new MediatorActivity();
#endif
            activity?.SetTag("request.type", requestType.Name);
            activity?.SetTag("result.type", resultType.Name);

            try
            {
                var handlerInfo = GetInfo(requestType, resultType);

                activity?.SetTag("handler.type", handlerInfo.Type);

                var result = await handlerInfo.InvokeAsync(_serviceProvider, request, cancellationToken);

                activity?.SetTag("result.success", result.Problem == null);
                activity?.SetTag("result.Status", result.Status);
                activity?.SetOk();

                return result;
            }
            catch (Exception ex)
            {
                if (ex is HandlerNotFoundException)
                    throw;

                activity?.SetError(ex);

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

            public async Task<TResult> InvokeAsync<TResult>(IServiceProvider services, IRequest<TResult> request, CancellationToken cancellationToken) where TResult : IResult
            {
                try
                {
                    var instance = services.GetService(Type) ?? throw new HandlerNotFoundException(Type);
                    var resultTask = handleMethod.Invoke(instance, [request, cancellationToken]);
                    return await (Task<TResult>)resultTask!;
                }
                catch (TargetInvocationException ex)
                {
                    var inner = ex.InnerException ?? ex;
                    ExceptionDispatchInfo.Capture(inner).Throw();
                    return default!;
                }
            }
        }
    }
}