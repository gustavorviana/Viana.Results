#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Viana.Results.Mediators
{
    internal static class ActivityExtension
    {
        internal static Activity AddException(this Activity activity, Exception exception, in TagList tags = default, DateTimeOffset timestamp = default)
        {
            if (activity == null)
                return activity;

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            TagList exceptionTags = tags;

            const string ExceptionEventName = "exception";
            const string ExceptionMessageTag = "exception.message";
            const string ExceptionStackTraceTag = "exception.stacktrace";
            const string ExceptionTypeTag = "exception.type";

            bool hasMessage = false;
            bool hasStackTrace = false;
            bool hasType = false;

            for (int i = 0; i < exceptionTags.Count; i++)
            {
                if (exceptionTags[i].Key == ExceptionMessageTag)
                {
                    hasMessage = true;
                }
                else if (exceptionTags[i].Key == ExceptionStackTraceTag)
                {
                    hasStackTrace = true;
                }
                else if (exceptionTags[i].Key == ExceptionTypeTag)
                {
                    hasType = true;
                }
            }

            if (!hasMessage)
                exceptionTags.Add(new KeyValuePair<string, object>(ExceptionMessageTag, exception.Message));

            if (!hasStackTrace)
                exceptionTags.Add(new KeyValuePair<string, object>(ExceptionStackTraceTag, exception.ToString()));

            if (!hasType)
                exceptionTags.Add(new KeyValuePair<string, object>(ExceptionTypeTag, exception.GetType().ToString()));

            return activity.AddEvent(new ActivityEvent(ExceptionEventName, timestamp, [.. tags]));
        }
    }
}
#endif
