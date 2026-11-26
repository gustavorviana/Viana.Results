using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Viana.Results.AspNetCore.Filters;

/// <summary>
/// Result filter that lets actions return Viana results directly: an <see cref="ObjectResult"/> whose value
/// is an <see cref="IResult"/> is replaced by a <see cref="VianaActionResult"/>. Other results are untouched.
/// </summary>
public sealed class VianaResultFilter : IResultFilter
{
    /// <inheritdoc />
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: IResult result })
            context.Result = new VianaActionResult(result);
    }

    /// <inheritdoc />
    public void OnResultExecuted(ResultExecutedContext context) { }
}
