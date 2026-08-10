using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.Mvc;

/// <summary>
/// Maps a Viana <see cref="ProblemResult"/> onto ASP.NET Core's native
/// <see cref="ProblemDetails"/> model so the framework's own serialization
/// (RFC 9457, "application/problem+json") can write the response.
/// </summary>
public static class ProblemDetailsMapper
{
    /// <summary>
    /// Converts a <see cref="ProblemResult"/> into a <see cref="ProblemDetails"/>.
    /// The "detail" extension (or, as a fallback, "description") becomes <see cref="ProblemDetails.Detail"/>,
    /// "instance" becomes <see cref="ProblemDetails.Instance"/>, and every other extension member
    /// (e.g. "errors") is copied into <see cref="ProblemDetails.Extensions"/>.
    /// </summary>
    /// <param name="problem">The source problem. Must not be null.</param>
    /// <returns>An equivalent <see cref="ProblemDetails"/>.</returns>
    public static ProblemDetails ToProblemDetails(this ProblemResult problem)
    {
        if (problem == null)
            throw new ArgumentNullException(nameof(problem));

        var details = new ProblemDetails
        {
            Type = problem.Type,
            Title = problem.Title,
            Status = problem.Status,
            Detail = AsString(problem.Extensions, "detail") ?? AsString(problem.Extensions, "description"),
            Instance = AsString(problem.Extensions, "instance"),
        };

        foreach (var kv in problem.Extensions)
        {
            if (IsMappedMember(kv.Key))
                continue;

            details.Extensions[kv.Key] = kv.Value;
        }

        return details;
    }

    private static bool IsMappedMember(string key) =>
        key.Equals("detail", StringComparison.OrdinalIgnoreCase)
        || key.Equals("description", StringComparison.OrdinalIgnoreCase)
        || key.Equals("instance", StringComparison.OrdinalIgnoreCase);

    private static string? AsString(IReadOnlyDictionary<string, object?> extensions, string key) =>
        extensions.TryGetValue(key, out var value) ? value as string ?? value?.ToString() : null;
}
