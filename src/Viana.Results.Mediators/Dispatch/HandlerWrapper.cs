using System;
using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.Mediators.Dispatch;

/// <summary>Typed dispatch entry for one (request type, result type); built once, reused process-wide.</summary>
internal abstract class HandlerWrapper<TResult> where TResult : IResult
{
    protected HandlerWrapper(Type requestType)
    {
        RequestTypeName = TypeNames.Get(requestType);
        ResultTypeName = TypeNames.Get(typeof(TResult));
        SpanName = "Mediator.Send " + RequestTypeName;
    }

    /// <summary>Span name (computed once).</summary>
    public string SpanName { get; }

    /// <summary>Friendly request type name (telemetry).</summary>
    public string RequestTypeName { get; }

    /// <summary>Friendly result type name (telemetry).</summary>
    public string ResultTypeName { get; }

    /// <summary>Resolves the handler; throws <see cref="HandlerNotFoundException"/> when missing.</summary>
    public abstract object ResolveHandler(IServiceProvider services);

    /// <summary>Calls the handler; a null task becomes an <see cref="InvalidOperationException"/>.</summary>
    public abstract Task<TResult> Invoke(object handler, IRequest<TResult> request, CancellationToken cancellationToken);
}

internal sealed class HandlerWrapper<TRequest, TResult> : HandlerWrapper<TResult>
    where TRequest : IRequest<TResult>
    where TResult : IResult
{
    public HandlerWrapper() : base(typeof(TRequest))
    {
    }

    public override object ResolveHandler(IServiceProvider services)
        => services.GetService(typeof(IHandler<TRequest, TResult>))
           ?? throw new HandlerNotFoundException(typeof(TRequest), typeof(TResult));

    public override Task<TResult> Invoke(object handler, IRequest<TResult> request, CancellationToken cancellationToken)
        => ((IHandler<TRequest, TResult>)handler).Handle((TRequest)request, cancellationToken)
           ?? throw new InvalidOperationException(
               $"Handler {TypeNames.Get(handler.GetType())} returned a null task for {RequestTypeName}.");
}
