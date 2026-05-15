using System;
using System.Linq;

namespace Viana.Results;

internal static class ResultReflections
{
    /// <summary>
    /// Returns true when the result wrapper should be unwrapped in OpenAPI/MVC payloads
    /// (i.e. the response body is the inner data rather than the wrapper). <c>PagedResult</c>
    /// is excluded because it carries metadata (PageNumber/TotalPages) that must remain visible.
    /// </summary>
    public static bool IsUnwrappableType(Type type)
    {
        if (!typeof(IResultData).IsAssignableFrom(type) ||
            typeof(IHasExtensions).IsAssignableFrom(type) ||
            !type.IsGenericType)
            return false;

        var hasGenericResult = false;

        foreach (var iface in type.GetInterfaces().Where(x => x.IsGenericType))
        {
            var parameter = iface.GetGenericTypeDefinition();
            if (parameter == typeof(IPagedResult<>))
                return false;

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

        var underlying = Nullable.GetUnderlyingType(type);

        type = underlying ?? type;
        return underlying != null ||
            type.IsEnum ||
            type.IsValueType ||
            type == typeof(string) ||
            type == typeof(Uri) ||
            type == typeof(Version);
    }
}
