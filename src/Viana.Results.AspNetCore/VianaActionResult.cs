using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Viana.Results.AspNetCore;

/// <summary>
/// Action result that writes a Viana <see cref="IResult"/> through the native ASP.NET Core pipeline.
/// <para>
/// Failures (<see cref="IResult.Problem"/> set) become a native <see cref="ProblemDetails"/> written by
/// <see cref="IProblemDetailsService"/> (or <see cref="ProblemDetailsFactory"/> when the service is not
/// registered), so native defaults and <c>CustomizeProblemDetails</c> apply. Precedence: Viana values,
/// then native defaults for missing members, then <c>CustomizeProblemDetails</c>. The native
/// <c>traceId</c> always wins; a Viana <c>traceId</c> is only used when the native pipeline provides none.
/// </para>
/// <para>
/// Successes go through normal MVC formatting (<see cref="ObjectResult"/>), so content negotiation and
/// the configured formatters apply. The HTTP status is always <see cref="IResult.Status"/>.
/// </para>
/// </summary>
/// <param name="result">The Viana result to write.</param>
public sealed class VianaActionResult(IResult result) : IActionResult
{
    private const string ProblemContentType = "application/problem+json";

    /// <summary>Gets the wrapped Viana result.</summary>
    public IResult Result { get; } = result ?? throw new ArgumentNullException(nameof(result));

    /// <inheritdoc />
    public Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return Result.Problem != null
            ? WriteProblemAsync(context)
            : WriteSuccessAsync(context);
    }

    private Task WriteSuccessAsync(ActionContext context)
    {
        if (!ResultResponseBody.HasBody(Result))
            return new StatusCodeResult(Result.Status).ExecuteResultAsync(context);

        // A null value follows the MVC default for null objects (204 unless TreatNullValueAsNoContent is off).
        return new ObjectResult(ResultResponseBody.GetBody(Result)) { StatusCode = Result.Status }.ExecuteResultAsync(context);
    }

    private async Task WriteProblemAsync(ActionContext context)
    {
        var httpContext = context.HttpContext;
        var services = httpContext.RequestServices;
        var namingPolicy = services.GetService<IOptions<JsonOptions>>()?.Value.JsonSerializerOptions.PropertyNamingPolicy;

        var details = ProblemDetailsMapper.ToProblemDetails(Result, namingPolicy, out var vianaTraceId);
        httpContext.Response.StatusCode = Result.Status;

        var problemService = services.GetService<IProblemDetailsService>();
        if (problemService != null)
        {
            // The native trace id always wins (same source as the framework factory); the Viana one is a fallback.
            // net8.0's writer does not add it, net10.0's overwrites it: setting it here keeps both identical.
            details.Extensions[ProblemDetailsMapper.TraceIdName] = NativeTraceId(httpContext) ?? vianaTraceId;
        }

        if (problemService != null &&
            await problemService.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = details }))
            return;

        // No problem details service: use the native factory defaults, then the Viana values on top.
        var factory = services.GetService<ProblemDetailsFactory>();
        var output = factory?.CreateProblemDetails(httpContext, Result.Status, details.Title, details.Type, details.Detail, details.Instance)
            ?? details;

        var extensions = new Dictionary<string, object?>(output.Extensions, StringComparer.OrdinalIgnoreCase);
        foreach (var member in details.Extensions)
            extensions[member.Key] = member.Value;
        if (!extensions.ContainsKey(ProblemDetailsMapper.TraceIdName) && vianaTraceId != null)
            extensions[ProblemDetailsMapper.TraceIdName] = NativeTraceId(httpContext) ?? vianaTraceId;
        output.Extensions = extensions;
        output.Status = Result.Status;

        var objectResult = new ObjectResult(output) { StatusCode = Result.Status };
        objectResult.ContentTypes.Add(ProblemContentType);
        await objectResult.ExecuteResultAsync(context);
    }

    private static string? NativeTraceId(HttpContext httpContext)
        => System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;
}
