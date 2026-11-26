#if NET6_0_OR_GREATER
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Viana.Results.Mediators.Dispatch;

namespace Viana.Results.Mediators.Telemetry;

/// <summary>Mediator traces and metrics (net6.0+). Nothing is built unless a listener asks for it.</summary>
internal static class MediatorTelemetry
{
    private static readonly string Version =
        typeof(MediatorTelemetry).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(MediatorTelemetry).Assembly.GetName().Version?.ToString()
        ?? "1.0.0";

    private static readonly ActivitySource Source = new(MediatorDiagnostics.SourceName, Version);
    private static readonly Meter Meter = new(MediatorDiagnostics.MeterName, Version);
#if NET9_0_OR_GREATER
    // Seconds-based buckets (the SDK default boundaries are millisecond-style); an app View still wins.
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>(
        MediatorDiagnostics.DurationInstrument, "s", "Duration of mediator dispatches.", tags: null,
        advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = MediatorDiagnostics.DurationBucketBoundaries });
#else
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>(
        MediatorDiagnostics.DurationInstrument, "s", "Duration of mediator dispatches.");
#endif

    /// <summary>True when an enabled telemetry kind has a listener (otherwise the fast path is used).</summary>
    public static bool IsActive(MediatorTelemetryOptions options)
        => (options.Tracing && Source.HasListeners()) || (options.Metrics && Duration.Enabled);

    public static Activity? Start<TResult>(MediatorTelemetryOptions options, HandlerWrapper<TResult> wrapper) where TResult : IResult
        => options.Tracing ? Source.StartActivity(wrapper.SpanName, ActivityKind.Internal) : null;

    public static void Complete<TResult>(Activity? activity, MediatorTelemetryOptions options, HandlerWrapper<TResult> wrapper,
        string handlerName, TResult result, long started) where TResult : IResult
    {
        var success = result.Problem == null;
        if (activity is { IsAllDataRequested: true })
        {
            SetCommonTags(activity, wrapper, handlerName);
            activity.SetTag(MediatorDiagnostics.SuccessTag, success);
            activity.SetTag(MediatorDiagnostics.StatusTag, result.Status);
        }

        if (activity != null)
        {
            if (!success && result.Status >= 500)
                activity.SetStatus(ActivityStatusCode.Error, result.Problem!.Title);
            else
                activity.SetStatus(ActivityStatusCode.Ok);
        }

        if (options.Metrics && Duration.Enabled)
        {
            var tags = new TagList
            {
                { MediatorDiagnostics.RequestTypeTag, wrapper.RequestTypeName },
                { MediatorDiagnostics.StatusTag, result.Status },
                { MediatorDiagnostics.SuccessTag, success }
            };
            Duration.Record(Elapsed(started), tags);
        }
    }

    public static void Fail<TResult>(Activity? activity, MediatorTelemetryOptions options, HandlerWrapper<TResult> wrapper,
        string? handlerName, Exception exception, long started) where TResult : IResult
    {
        var errorType = exception.GetType().FullName ?? exception.GetType().Name;
        var cancelled = exception is OperationCanceledException;

        if (activity != null)
        {
            if (activity.IsAllDataRequested)
            {
                SetCommonTags(activity, wrapper, handlerName);
                activity.SetTag(MediatorDiagnostics.SuccessTag, false);
                activity.SetTag(MediatorDiagnostics.ErrorTypeTag, errorType);
                if (cancelled)
                    activity.SetTag(MediatorDiagnostics.CancelledTag, true);
            }

            if (!cancelled)
            {
                var level = options.ExceptionDetails;
                activity.SetStatus(ActivityStatusCode.Error, level switch
                {
                    ExceptionDetailLevel.Full => exception.Message,
                    ExceptionDetailLevel.TypeOnly => errorType,
                    _ => null
                });
                if (activity.IsAllDataRequested)
                    RecordException(activity, exception, errorType, level);
            }
        }

        if (options.Metrics && Duration.Enabled)
        {
            var tags = new TagList
            {
                { MediatorDiagnostics.RequestTypeTag, wrapper.RequestTypeName },
                { MediatorDiagnostics.SuccessTag, false },
                { MediatorDiagnostics.ErrorTypeTag, errorType }
            };
            Duration.Record(Elapsed(started), tags);
        }
    }

    private static void SetCommonTags<TResult>(Activity activity, HandlerWrapper<TResult> wrapper, string? handlerName) where TResult : IResult
    {
        activity.SetTag(MediatorDiagnostics.RequestTypeTag, wrapper.RequestTypeName);
        activity.SetTag(MediatorDiagnostics.ResultTypeTag, wrapper.ResultTypeName);
        if (handlerName != null)
            activity.SetTag(MediatorDiagnostics.HandlerTypeTag, handlerName);
    }

    private static void RecordException(Activity activity, Exception exception, string errorType, ExceptionDetailLevel level)
    {
        switch (level)
        {
            case ExceptionDetailLevel.Full:
#if NET9_0_OR_GREATER
                activity.AddException(exception);
#else
                activity.AddEvent(new ActivityEvent("exception", default, new ActivityTagsCollection
                {
                    { "exception.type", errorType },
                    { "exception.message", exception.Message },
                    { "exception.stacktrace", exception.ToString() }
                }));
#endif
                break;
            case ExceptionDetailLevel.TypeOnly:
                activity.AddEvent(new ActivityEvent("exception", default, new ActivityTagsCollection { { "exception.type", errorType } }));
                break;
        }
    }

    private static double Elapsed(long started)
        => (Stopwatch.GetTimestamp() - started) / (double)Stopwatch.Frequency;
}
#endif
