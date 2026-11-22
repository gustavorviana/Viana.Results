using System;
using System.Threading.Tasks;

namespace Viana.Results.AspNetCore;

/// <summary>Converts Viana results to minimal API results.</summary>
public static class HttpResultExtensions
{
    /// <summary>Converts <paramref name="result"/> to a minimal API result (see <see cref="VianaHttpResult"/>).</summary>
    /// <param name="result">The Viana result.</param>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this IResult result) => new VianaHttpResult(result);

    /// <summary>Awaits <paramref name="result"/> and converts it to a minimal API result.</summary>
    /// <typeparam name="TResult">The Viana result type.</typeparam>
    /// <param name="result">The task producing the Viana result.</param>
    public static async Task<Microsoft.AspNetCore.Http.IResult> ToHttpResult<TResult>(this Task<TResult> result) where TResult : IResult
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        return new VianaHttpResult(await result.ConfigureAwait(false));
    }
}
