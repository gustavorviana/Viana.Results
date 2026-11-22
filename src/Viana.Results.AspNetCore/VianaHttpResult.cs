using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using HttpResults = Microsoft.AspNetCore.Http.TypedResults;

namespace Viana.Results.AspNetCore;

/// <summary>
/// Minimal API result that writes a Viana <see cref="Viana.Results.IResult"/> with the same rules as
/// <see cref="VianaActionResult"/>: failures through the native problem details pipeline, successes as
/// JSON with the minimal API JSON options. The HTTP status is always <see cref="Viana.Results.IResult.Status"/>;
/// a null value answers 204 (aligned with the MVC default).
/// </summary>
/// <param name="result">The Viana result to write.</param>
public sealed class VianaHttpResult(Viana.Results.IResult result) : Microsoft.AspNetCore.Http.IResult
{
    /// <summary>Gets the wrapped Viana result.</summary>
    public Viana.Results.IResult Result { get; } = result ?? throw new ArgumentNullException(nameof(result));

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

        if (Result.Problem != null)
        {
            var namingPolicy = httpContext.RequestServices.GetService<IOptions<HttpJsonOptions>>()?.Value.SerializerOptions.PropertyNamingPolicy;
            return ResultResponseWriter.WriteProblemAsync(httpContext, Result, namingPolicy, (details, vianaTraceId) =>
            {
                // No problem details service: the native minimal API problem result (native defaults).
                ResultResponseWriter.MergeExtensions(details, details, vianaTraceId, httpContext);
                details.Status = Result.Status;
                return HttpResults.Problem(details).ExecuteAsync(httpContext);
            });
        }

        if (!ResultResponseBody.HasBody(Result))
            return HttpResults.StatusCode(Result.Status).ExecuteAsync(httpContext);

        var body = ResultResponseBody.GetBody(Result);
        if (body == null)
            return HttpResults.NoContent().ExecuteAsync(httpContext);

        return HttpResults.Json(body, (System.Text.Json.JsonSerializerOptions?)null, contentType: null, statusCode: Result.Status).ExecuteAsync(httpContext);
    }
}
