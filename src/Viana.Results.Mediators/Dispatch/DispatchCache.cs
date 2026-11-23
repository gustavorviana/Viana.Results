using System;
using System.Collections.Concurrent;

namespace Viana.Results.Mediators.Dispatch;

/// <summary>Process-wide dispatch entries per request runtime type; the result type is the generic argument.</summary>
internal static class DispatchCache<TResult> where TResult : IResult
{
    private static readonly ConcurrentDictionary<Type, HandlerWrapper<TResult>> Entries = new();

    public static HandlerWrapper<TResult> Get(Type requestType)
        => Entries.GetOrAdd(requestType, static type =>
            (HandlerWrapper<TResult>)Activator.CreateInstance(
                typeof(HandlerWrapper<,>).MakeGenericType(type, typeof(TResult)))!);
}
