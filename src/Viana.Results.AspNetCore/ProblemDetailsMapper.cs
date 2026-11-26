using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.AspNetCore;

/// <summary>Maps a failed Viana result to a native <see cref="ProblemDetails"/>.</summary>
internal static class ProblemDetailsMapper
{
    internal const string TraceIdName = "traceId";

    /// <summary>
    /// Builds a native <see cref="ProblemDetails"/> from <paramref name="result"/> (which must have a problem).
    /// Status comes from <see cref="IResult.Status"/>; "about:blank" becomes null so the native type link applies;
    /// "detail"/"instance" are promoted; other extension names follow <paramref name="namingPolicy"/> in a
    /// case-insensitive dictionary (last value wins). A Viana "traceId" (any casing) is removed and returned in
    /// <paramref name="vianaTraceId"/>: the native trace id always wins and the Viana one is only a fallback.
    /// </summary>
    internal static ProblemDetails ToProblemDetails(IResult result, JsonNamingPolicy? namingPolicy, out object? vianaTraceId)
    {
        var problem = result.Problem ?? throw new ArgumentException("The result has no problem.", nameof(result));
        vianaTraceId = null;

        var details = new ProblemDetails
        {
            Status = result.Status,
            Title = problem.Title,
            Type = string.Equals(problem.Type, "about:blank", StringComparison.OrdinalIgnoreCase) ? null : problem.Type,
            Extensions = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        };

        foreach (var member in problem.Extensions)
        {
            if (member.Key.Equals("detail", StringComparison.OrdinalIgnoreCase) && member.Value is string detail)
            {
                details.Detail = detail;
                continue;
            }

            if (member.Key.Equals("instance", StringComparison.OrdinalIgnoreCase) && member.Value is string instance)
            {
                details.Instance = instance;
                continue;
            }

            if (member.Key.Equals(TraceIdName, StringComparison.OrdinalIgnoreCase))
            {
                vianaTraceId = member.Value;
                continue;
            }

            var name = namingPolicy?.ConvertName(member.Key) ?? member.Key;
            details.Extensions[name] = member.Value;
        }

        return details;
    }
}
