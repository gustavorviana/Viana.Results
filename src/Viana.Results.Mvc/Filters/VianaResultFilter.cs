using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Viana.Results.Mvc.Filters;

/// <summary>
/// MVC result filter that intercepts action results whose value is an <see cref="IResult"/>
/// and replaces them with native ASP.NET Core results, letting the framework serialize:
/// on error, the <see cref="ProblemResult"/> is mapped to <see cref="ProblemDetails"/> and offered
/// under the configured problem content types (default "application/problem+json"); on success, the
/// payload is written by the configured output formatters; when there is no body, a bare
/// <see cref="StatusCodeResult"/> is used.
/// </summary>
public class VianaResultFilter : IResultFilter
{
    private static readonly string[] DefaultProblemContentTypes = { "application/problem+json" };

    private readonly string[] _problemContentTypes;

    /// <summary>Initializes the filter with the default problem content type ("application/problem+json").</summary>
    public VianaResultFilter()
        : this((IEnumerable<string>?)null)
    {
    }

    /// <summary>Initializes the filter from configured <see cref="VianaResultMvcOptions"/>.</summary>
    /// <param name="options">The MVC integration options.</param>
    public VianaResultFilter(IOptions<VianaResultMvcOptions> options)
        : this(options?.Value?.ProblemContentTypes)
    {
    }

    private VianaResultFilter(IEnumerable<string>? problemContentTypes)
    {
        var configured = problemContentTypes?
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToArray();

        _problemContentTypes = configured != null && configured.Length > 0
            ? configured
            : DefaultProblemContentTypes;
    }

    /// <inheritdoc />
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is IResult result)
            context.Result = ToActionResult(result);
    }

    /// <inheritdoc />
    public void OnResultExecuted(ResultExecutedContext context) { }

    /// <summary>
    /// Translates an <see cref="IResult"/> into the native <see cref="IActionResult"/> that
    /// represents it, so serialization and content negotiation are handled by the framework.
    /// </summary>
    /// <param name="result">The result to translate.</param>
    /// <returns>A native action result carrying the status code and body (when any).</returns>
    public IActionResult ToActionResult(IResult result)
    {
        if (result.Problem != null)
        {
            var problem = new ObjectResult(result.Problem.ToProblemDetails())
            {
                StatusCode = result.Status,
            };

            foreach (var contentType in _problemContentTypes)
                problem.ContentTypes.Add(contentType);

            return problem;
        }

        var body = ResultResponseBody.GetBody(result);
        if (body == null)
            return new StatusCodeResult(result.Status);

        return new ObjectResult(body) { StatusCode = result.Status };
    }
}
