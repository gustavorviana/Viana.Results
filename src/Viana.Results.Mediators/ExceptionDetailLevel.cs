namespace Viana.Results.Mediators;

/// <summary>Exception details recorded in telemetry (messages and stack traces may contain sensitive data).</summary>
public enum ExceptionDetailLevel
{
    /// <summary>Type, message and stack trace.</summary>
    Full,

    /// <summary>Only the exception type (no message, no stack trace).</summary>
    TypeOnly,

    /// <summary>No exception event; the span still ends with error status and <c>error.type</c>.</summary>
    None
}
