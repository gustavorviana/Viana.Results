using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Viana.Results.Mvc.Filters;

/// <summary>
/// MVC result filter that intercepts action results whose value is an <see cref="IResult"/>.
/// Replaces the <see cref="ObjectResult"/> with a <see cref="VianaResultAction"/> so that
/// the result is written as JSON (status code, problem details on error, or data on success).
/// </summary>
public class VianaResultFilter : IResultFilter
{
    /// <inheritdoc />
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is IResult result)
            context.Result = new VianaResultAction(result);
    }

    /// <inheritdoc />
    public void OnResultExecuted(ResultExecutedContext context) { }
}
