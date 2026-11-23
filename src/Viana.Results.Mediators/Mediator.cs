using System;
using System.Threading;
using System.Threading.Tasks;
using Viana.Results.Mediators.Dispatch;
#if NET6_0_OR_GREATER
using System.Diagnostics;
using Viana.Results.Mediators.Telemetry;
#endif

namespace Viana.Results.Mediators
{
    /// <summary>
    /// Implementation of the mediator pattern for dispatching requests to their corresponding handlers.
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MediatorOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class with default options.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve handler instances.</param>
        public Mediator(IServiceProvider serviceProvider) : this(serviceProvider, new MediatorOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve handler instances.</param>
        /// <param name="options">Mediator options (telemetry switches and exception detail level).</param>
        public Mediator(IServiceProvider serviceProvider, MediatorOptions options)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Sends a request to the appropriate handler and returns the result.
        /// Every error (null request, missing handler, handler failure) surfaces through the returned task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the handler result.</returns>
        /// <exception cref="HandlerNotFoundException">No handler is registered for the request and result type (through the task).</exception>
        public Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default) where TResult : IResult
        {
            try
            {
                if (request is null)
                    throw new ArgumentNullException(nameof(request));

                var wrapper = DispatchCache<TResult>.Get(request.GetType());
#if NET6_0_OR_GREATER
                if (MediatorTelemetry.IsActive(_options.Telemetry))
                    return SendObservedAsync(wrapper, request, cancellationToken);
#endif
                return wrapper.Invoke(wrapper.ResolveHandler(_serviceProvider), request, cancellationToken);
            }
            catch (Exception ex)
            {
                return Task.FromException<TResult>(ex);
            }
        }

#if NET6_0_OR_GREATER
        private async Task<TResult> SendObservedAsync<TResult>(HandlerWrapper<TResult> wrapper, IRequest<TResult> request, CancellationToken cancellationToken)
            where TResult : IResult
        {
            var telemetry = _options.Telemetry;
            var started = Stopwatch.GetTimestamp();
            using var activity = MediatorTelemetry.Start(telemetry, wrapper);
            string? handlerName = null;
            try
            {
                var handler = wrapper.ResolveHandler(_serviceProvider);
                handlerName = TypeNames.Get(handler.GetType());
                var result = await wrapper.Invoke(handler, request, cancellationToken).ConfigureAwait(false);
                MediatorTelemetry.Complete(activity, telemetry, wrapper, handlerName, result, started);
                return result;
            }
            catch (Exception ex)
            {
                MediatorTelemetry.Fail(activity, telemetry, wrapper, handlerName, ex, started);
                throw;
            }
        }
#endif
    }
}
