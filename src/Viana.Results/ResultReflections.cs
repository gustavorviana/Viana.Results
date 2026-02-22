using System;
using System.Linq;

namespace Viana.Results;

internal static class ResultReflections
{
    public static bool IsUnwrappableType(Type type)
    {
        if (!typeof(IResultData).IsAssignableFrom(type) || !type.IsGenericType)
            return false;

        var hasGenericResult = false;

        foreach (var iface in type.GetInterfaces().Where(x => x.IsGenericType))
        {
            var parameter = iface.GetGenericTypeDefinition();
            if (parameter == typeof(IListResult<>))
                return false;

            if (!hasGenericResult && parameter == typeof(IResult<>))
                hasGenericResult = true;
        }

        return hasGenericResult;
    }

    public static bool IsScalarLike(Type type)
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
