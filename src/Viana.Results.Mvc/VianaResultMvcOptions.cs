using System.Collections.Generic;

namespace Viana.Results.Mvc;

/// <summary>
/// Options for the Viana.Results MVC integration.
/// </summary>
public sealed class VianaResultMvcOptions
{
    /// <summary>
    /// Content types offered (in order) for content negotiation when writing an RFC 9457 problem
    /// response. Defaults to a single entry, "application/problem+json". Add "application/problem+xml"
    /// to also negotiate XML when an XML output formatter is registered, or clear and replace to
    /// fully customize. When left empty, "application/problem+json" is used.
    /// </summary>
    public IList<string> ProblemContentTypes { get; } = new List<string> { "application/problem+json" };
}
