using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.AspNetCore;

/// <summary>Converts Viana results to MVC action results.</summary>
public static class ResultActionResultExtensions
{
    /// <summary>
    /// Converts <paramref name="result"/> to an <see cref="IActionResult"/> written through the native
    /// ASP.NET Core pipeline (see <see cref="VianaActionResult"/>).
    /// </summary>
    /// <param name="result">The Viana result (any <see cref="IResult"/>: Result, Result&lt;T&gt;, ListResult&lt;T&gt;, PagedResult&lt;T&gt;).</param>
    public static IActionResult ToActionResult(this IResult result) => new VianaActionResult(result);

    /// <summary>Awaits <paramref name="result"/> and converts it to an <see cref="IActionResult"/>.</summary>
    /// <typeparam name="TResult">The Viana result type.</typeparam>
    /// <param name="result">The task producing the Viana result.</param>
    public static async Task<IActionResult> ToActionResult<TResult>(this Task<TResult> result) where TResult : IResult
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        return new VianaActionResult(await result.ConfigureAwait(false));
    }
}
