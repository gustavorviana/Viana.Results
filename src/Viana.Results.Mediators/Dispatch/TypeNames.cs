using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Viana.Results.Mediators.Dispatch;

/// <summary>Cached friendly type names (<c>Result&lt;User&gt;</c> instead of <c>Result`1</c>).</summary>
internal static class TypeNames
{
    private static readonly ConcurrentDictionary<Type, string> Names = new();

    public static string Get(Type type) => Names.GetOrAdd(type, static t => Format(t));

    private static string Format(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;
        var name = type.Name;
        var tick = name.IndexOf('`');
        if (tick >= 0)
            name = name.Substring(0, tick);
        return name + "<" + string.Join(", ", type.GetGenericArguments().Select(Format)) + ">";
    }
}
