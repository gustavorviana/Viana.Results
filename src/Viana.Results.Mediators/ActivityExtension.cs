#if NET5_0_OR_GREATER
using System;
using System.Diagnostics;

namespace Viana.Results.Mediators;

internal static class ActivityExtension
{
    internal static Activity AddException(this Activity activity, Exception exception)
    {
        if (exception is null)
            throw new ArgumentNullException(nameof(exception));

        var tags = new ActivityTagsCollection
        {
            { "exception.message", exception.Message },
            { "exception.stacktrace", exception.ToString() ?? "" },
            { "exception.type", exception.GetType().ToString() ?? "" }
        };
        activity.AddEvent(new ActivityEvent("exception", default, tags));
        return activity;
    }
}
#endif
