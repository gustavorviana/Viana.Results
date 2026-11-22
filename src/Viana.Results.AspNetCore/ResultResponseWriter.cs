using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Viana.Results.AspNetCore;

/// <summary>Problem writing shared by MVC (<see cref="VianaActionResult"/>) and minimal APIs (<see cref="VianaHttpResult"/>).</summary>
internal static class ResultResponseWriter
{
    /// <summary>
    /// Writes a failed <paramref name="result"/> as native problem details: Viana values first, native
    /// defaults for missing members, <c>CustomizeProblemDetails</c> last; the native trace id always wins.
    /// When no <see cref="IProblemDetailsService"/> can write, <paramref name="fallback"/> receives the
    /// mapped details (with trace id) and writes them with the host's own mechanism.
    /// </summary>
    internal static async Task WriteProblemAsync(HttpContext httpContext, IResult result, JsonNamingPolicy? namingPolicy, Func<ProblemDetails, object?, Task> fallback)
    {
        var details = ProblemDetailsMapper.ToProblemDetails(result, namingPolicy, out var vianaTraceId);
        httpContext.Response.StatusCode = result.Status;

        var problemService = httpContext.RequestServices.GetService<IProblemDetailsService>();
        if (problemService != null)
        {
            // The native trace id always wins (same source as the framework factory); the Viana one is a fallback.
            // net8.0's writer does not add it, net10.0's overwrites it: setting it here keeps both identical.
            details.Extensions[ProblemDetailsMapper.TraceIdName] = NativeTraceId(httpContext) ?? vianaTraceId;
            if (await problemService.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = details }))
                return;
        }

        await fallback(details, vianaTraceId);
    }

    /// <summary>Overlays the Viana extensions on native defaults and applies the trace id rule.</summary>
    internal static void MergeExtensions(ProblemDetails target, ProblemDetails viana, object? vianaTraceId, HttpContext httpContext)
    {
        var extensions = new Dictionary<string, object?>(target.Extensions, StringComparer.OrdinalIgnoreCase);
        foreach (var member in viana.Extensions)
        {
            if (!member.Key.Equals(ProblemDetailsMapper.TraceIdName, StringComparison.OrdinalIgnoreCase))
                extensions[member.Key] = member.Value;
        }
        if (!extensions.ContainsKey(ProblemDetailsMapper.TraceIdName))
            extensions[ProblemDetailsMapper.TraceIdName] = NativeTraceId(httpContext) ?? vianaTraceId;
        target.Extensions = extensions;
    }

    internal static string? NativeTraceId(HttpContext httpContext)
        => System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;
}
