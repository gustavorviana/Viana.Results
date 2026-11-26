using Microsoft.Extensions.Logging;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Routes the <c>Viana.Results.OpenApi</c> category to a <see cref="ListLogger"/>.</summary>
public sealed class ListLoggerProvider : ILoggerProvider
{
    public ListLogger Logger { get; } = new();

    public ILogger CreateLogger(string categoryName)
        => categoryName == "Viana.Results.OpenApi" ? Logger : Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

    public void Dispose()
    {
    }
}
