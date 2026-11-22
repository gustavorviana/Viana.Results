using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Viana.Results.OpenApi.Schemas;

/// <summary>
/// Feature 008 FR-011: the problem type links ASP.NET Core writes when a problem has no type
/// (mirror of the internal <c>ProblemDetailsDefaults</c>; Viana.Results.AspNetCore sends <c>about:blank</c>
/// as null so the native defaults apply). Used so documented examples match the runtime.
/// </summary>
internal static class NativeProblemTypes
{
    private const string Rfc9110 = "https://tools.ietf.org/html/rfc9110#section-";

    private static readonly Dictionary<int, string> Links = new()
    {
        [400] = Rfc9110 + "15.5.1",
        [401] = Rfc9110 + "15.5.2",
        [403] = Rfc9110 + "15.5.4",
        [404] = Rfc9110 + "15.5.5",
        [405] = Rfc9110 + "15.5.6",
        [406] = Rfc9110 + "15.5.7",
        [408] = Rfc9110 + "15.5.9",
        [409] = Rfc9110 + "15.5.10",
        [412] = Rfc9110 + "15.5.13",
        [415] = Rfc9110 + "15.5.16",
        [422] = Rfc9110 + "15.5.21",
        [426] = Rfc9110 + "15.5.22",
        [500] = Rfc9110 + "15.6.1",
        [502] = Rfc9110 + "15.6.3",
        [503] = Rfc9110 + "15.6.4",
        [504] = Rfc9110 + "15.6.5",
    };

    /// <summary>Returns the type the runtime sends: the given type, or the native link when it is null/empty/about:blank.</summary>
    internal static string Resolve(string? type, int status)
    {
        if (!string.IsNullOrEmpty(type) && !string.Equals(type, ResultTypes.AboutBlank, StringComparison.OrdinalIgnoreCase))
            return type!;
        return Links.TryGetValue(status, out var link) ? link : ResultTypes.AboutBlank;
    }

    /// <summary>Applies <see cref="Resolve"/> to the <c>type</c> member of a serialized problem example.</summary>
    internal static void Apply(JsonNode? node, int status)
    {
        if (node is not JsonObject obj)
            return;
        string? current = obj["type"] is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;
        obj["type"] = Resolve(current, status);
    }
}
