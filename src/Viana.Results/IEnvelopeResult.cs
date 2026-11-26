namespace Viana.Results;

/// <summary>
/// A result whose whole object is the response body: every public property is written, except
/// <see cref="IResult.Status"/>, <see cref="IResult.Problem"/> and, for <see cref="IHasExtensions"/> types, the
/// <see cref="IHasExtensions.Extensions"/> container, whose entries are written at the root of the body.
/// </summary>
/// <typeparam name="T">The payload type, exposed as <see cref="IResult{T}.Data"/>.</typeparam>
/// <remarks>
/// Use it for list results with metadata, such as a cursor:
/// <code>
/// public sealed class CursorResult&lt;T&gt;(IReadOnlyList&lt;T&gt; data, string? nextCursor) : IEnvelopeResult&lt;IReadOnlyList&lt;T&gt;&gt;
/// {
///     public int Status =&gt; 200;
///     public ProblemResult? Problem =&gt; null;
///     public IReadOnlyList&lt;T&gt; Data =&gt; data;
///     public string? NextCursor =&gt; nextCursor;
/// }
/// // body: { "data": [...], "nextCursor": "abc" }
/// </code>
/// A failed envelope is written as its problem, and status 204 has no body, as for every result.
/// </remarks>
public interface IEnvelopeResult<out T> : IResult<T>
{
}
