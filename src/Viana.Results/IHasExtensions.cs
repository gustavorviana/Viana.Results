using System.Collections.Generic;

namespace Viana.Results;

/// <summary>
/// Marker contract for result types that carry RFC 9457 extension members in addition to the standard
/// "type", "title" and "status" fields.
/// </summary>
public interface IHasExtensions
{
    /// <summary>
    /// Gets the extension members of the problem details payload, keyed by member name.
    /// </summary>
    IReadOnlyDictionary<string, object?> Extensions { get; }
}