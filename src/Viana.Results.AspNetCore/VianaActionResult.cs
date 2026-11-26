using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
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
        return new ObjectResult(SuccessBody(context.HttpContext.RequestServices)) { StatusCode = Result.Status }.ExecuteResultAsync(context);
    }

    // Feature 014: envelopes are shaped for System.Text.Json when its output formatter is registered;
    // with other formatters only (Newtonsoft, XML) the instance itself is written.
    private object? SuccessBody(IServiceProvider services)
    {
        if (!ResultReflections.IsEnvelopeType(Result.GetType()))
            return ResultResponseBody.GetBody(Result);

        var formatters = services.GetService<IOptions<MvcOptions>>()?.Value.OutputFormatters;
        if (formatters == null || !HasSystemTextJson(formatters))
            return ResultResponseBody.GetBody(Result);

        var json = services.GetService<IOptions<JsonOptions>>()?.Value.JsonSerializerOptions ?? System.Text.Json.JsonSerializerOptions.Default;
        return ResultResponseBody.GetBody(Result, json);
    }

    // Indexed loop: no enumerator allocation on the response path.
    private static bool HasSystemTextJson(FormatterCollection<IOutputFormatter> formatters)
    {
        for (var i = 0; i < formatters.Count; i++)
        {
            if (formatters[i] is SystemTextJsonOutputFormatter)
                return true;
        }
        return false;
    }

    private Task WriteProblemAsync(ActionContext context)
    {
        var httpContext = context.HttpContext;
        var namingPolicy = httpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value.JsonSerializerOptions.PropertyNamingPolicy;

        return ResultResponseWriter.WriteProblemAsync(httpContext, Result, namingPolicy, (details, vianaTraceId) =>
        {
            // No problem details service: native MVC factory defaults, then the Viana values on top.
            var factory = httpContext.RequestServices.GetService<ProblemDetailsFactory>();
            var output = factory?.CreateProblemDetails(httpContext, Result.Status, details.Title, details.Type, details.Detail, details.Instance)
                ?? details;
            ResultResponseWriter.MergeExtensions(output, details, vianaTraceId, httpContext);
            output.Status = Result.Status;

            var objectResult = new ObjectResult(output) { StatusCode = Result.Status };
            objectResult.ContentTypes.Add(ProblemContentType);
            return objectResult.ExecuteResultAsync(context);
        });
    }
}
