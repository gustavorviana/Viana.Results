namespace Viana.Results.Examples.Shared.Catalog;

/// <summary>
/// A list result with a cursor, built on <see cref="IEnvelopeResult{T}"/>: the whole object is the body,
/// <c>{ "data": [...], "nextCursor": "..." }</c>, at runtime and in the OpenAPI document. No registration is needed.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed class CursorResult<T>(IReadOnlyList<T> data, string? nextCursor) : IEnvelopeResult<IReadOnlyList<T>>
{
    /// <inheritdoc />
    public int Status => 200;

    /// <inheritdoc />
    public ProblemResult? Problem => null;

    /// <summary>The items of this page.</summary>
    public IReadOnlyList<T> Data { get; } = data;

    /// <summary>Cursor of the next page, or null on the last page.</summary>
    public string? NextCursor { get; } = nextCursor;
}
