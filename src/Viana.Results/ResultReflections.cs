using System;
using System.Linq;

namespace Viana.Results;

internal static class ResultReflections
{
    /// <summary>Feature 014: whether <paramref name="type"/> implements <see cref="IEnvelopeResult{T}"/>.</summary>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2070",
        Justification = "Called with the runtime type of a result instance; the trimmer keeps the interfaces a constructed type implements when those interfaces are used, and IEnvelopeResult<> is referenced here.")]
#endif
    public static bool IsEnvelopeType(Type type)
        => EnvelopeTypes.GetOrAdd(type, static t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnvelopeResult<>)));

    // Hot path (every response): cached per type so GetInterfaces does not allocate per call.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, bool> EnvelopeTypes = new();

    /// <summary>
    /// Returns true when the result wrapper should be unwrapped in OpenAPI/MVC payloads
    /// (i.e. the response body is the inner data rather than the wrapper). Envelopes
    /// (<see cref="IEnvelopeResult{T}"/>, including paged results) are never unwrapped.
    /// </summary>
    public static bool IsUnwrappableType(Type type)
    {
        if (IsEnvelopeType(type) ||
            !typeof(IResultData).IsAssignableFrom(type) ||
            typeof(IHasExtensions).IsAssignableFrom(type) ||
            !type.IsGenericType)
            return false;

        var hasGenericResult = false;

        foreach (var iface in type.GetInterfaces().Where(x => x.IsGenericType))
        {
            var parameter = iface.GetGenericTypeDefinition();
            if (!hasGenericResult && parameter == typeof(IResult<>))
                hasGenericResult = true;
        }

        return hasGenericResult;
    }

    /// <summary>
    /// Returns the payload type (<c>T</c> in <c>IResult&lt;T&gt;</c>) for the given result type.
    /// For <c>Result&lt;User&gt;</c> returns <c>User</c>; for <c>ListResult&lt;User&gt;</c> returns <c>IReadOnlyList&lt;User&gt;</c>.
    /// </summary>
    public static Type? GetDataType(Type type)
    {
        return type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResult<>))
            ?.GetGenericArguments()[0];
    }

    public static bool IsScalarLike(Type? type)
    {
        if (type is null)
            return false;

        type = Nullable.GetUnderlyingType(type) ?? type;

        // Simple values only; any other struct is a structured DTO and is unwrapped like a class.
        return type.IsPrimitive ||
            type.IsEnum ||
            type == typeof(decimal) ||
            type == typeof(string) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type == typeof(Guid) ||
            type == typeof(Uri) ||
            type == typeof(Version) ||
            type.FullName == "System.DateOnly" ||
            type.FullName == "System.TimeOnly";
    }
}
