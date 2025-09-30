
using System;
using System.Diagnostics;

namespace Viana.Results.Mediators
{
    internal class MediatorActivity : IDisposable
    {
#if NET8_0_OR_GREATER
        private readonly Activity _activity;

        public MediatorActivity(ActivitySource source, Type requestType)
        {
            _activity = source.StartActivity($"Mediator.Send: {requestType.Name}", ActivityKind.Internal);
        }
#endif

        public MediatorActivity SetTag(string key, object value)
        {
#if NET8_0_OR_GREATER
            _activity?.SetTag(key, value);
#endif
            return this;
        }

        public MediatorActivity SetOk()
        {
#if NET8_0_OR_GREATER
            _activity?.SetStatus(ActivityStatusCode.Ok);
#endif
            return this;
        }

        public MediatorActivity SerError(Exception exception)
        {
#if NET8_0_OR_GREATER
            _activity?.SetStatus(ActivityStatusCode.Error);
            _activity?.AddException(exception);
#endif
            return this;
        }

        public void Dispose()
        {
#if NET8_0_OR_GREATER
            _activity?.Dispose();
#endif
        }
    }
}