using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Viana.Results;

/// <summary>
/// Represents an RFC 9457 "Problem Details" response payload for HTTP APIs.
///
/// Spec: RFC 9457 - Problem Details for HTTP APIs
/// https://datatracker.ietf.org/doc/html/rfc9457
/// </summary>
public sealed class ProblemResult
{
    /// <summary>RFC 9457: problem type URI.</summary>
    public string Type { get; }

    /// <summary>RFC 9457: short, human-readable summary of the problem type.</summary>
    public string Title { get; }

    /// <summary>RFC 9457: HTTP status code.</summary>
    public int Status { get; }

    /// <summary>RFC 9457 extension members. When description is provided via <see cref="WithDescription"/>, it is stored under key "description".</summary>
    public IReadOnlyDictionary<string, object?> Extensions { get; }

    /// <summary>
    /// Initializes a new instance with the specified status, title, type and extensions (no description).
    /// </summary>
    /// <param name="status">RFC 9457: HTTP status code.</param>
    /// <param name="title">RFC 9457: short, human-readable summary of the problem type.</param>
    /// <param name="type">RFC 9457: problem type URI. Defaults to "about:blank".</param>
    /// <param name="extensions">RFC 9457: optional extension members.</param>
    public ProblemResult(
        int status,
        string title,
        string type = "about:blank",
        IReadOnlyDictionary<string, object?>? extensions = null)
        : this(status, title, null, type, extensions)
    {
    }

    /// <summary>
    /// Private constructor with optional description (used by <see cref="WithDescription"/> when description is provided).
    /// </summary>
    private ProblemResult(
        int status,
        string title,
        string? description,
        string type,
        IReadOnlyDictionary<string, object?>? extensions)
    {
        Status = status;
        Type = string.IsNullOrWhiteSpace(type) ? "about:blank" : type;
        Title = string.IsNullOrWhiteSpace(title) ? "Error" : title;
        Extensions = NormalizeExtensions(extensions, description);
    }

    /// <summary>
    /// Creates a new instance. When <paramref name="description"/> is provided, it is stored in <see cref="Extensions"/> under key "description".
    /// </summary>
    /// <param name="status">RFC 9457: HTTP status code.</param>
    /// <param name="title">RFC 9457: short, human-readable summary of the problem type.</param>
    /// <param name="description">Optional. RFC 9457: human-readable explanation. Stored in <see cref="Extensions"/> when provided.</param>
    /// <param name="type">RFC 9457: problem type URI. Defaults to "about:blank".</param>
    /// <param name="extensions">RFC 9457: optional extension members.</param>
    /// <returns>A new <see cref="ProblemResult"/> instance.</returns>
    public static ProblemResult WithDescription(
        int status,
        string title,
        string? description = null,
        string type = "about:blank",
        IReadOnlyDictionary<string, object?>? extensions = null)
    {
        if (string.IsNullOrEmpty(description))
            return new ProblemResult(status, title, type, extensions);
        return new ProblemResult(status, title, description, type, extensions);
    }

    private static IReadOnlyDictionary<string, object?> NormalizeExtensions(IReadOnlyDictionary<string, object?>? extensions, string? description)
    {
        var copy = new Dictionary<string, object?>(StringComparer.Ordinal);

        if (extensions != null)
        {
            foreach (var kv in extensions)
            {
                if (IsStandardMember(kv.Key))
                    continue;

                copy[kv.Key] = kv.Value;
            }
        }

        if (!string.IsNullOrEmpty(description))
            copy["description"] = description;

        if (copy.Count == 0)
            return new Dictionary<string, object?>();

        return new ReadOnlyDictionary<string, object?>(copy);
    }

    private static bool IsStandardMember(string key)
    {
        return key.Equals("type", StringComparison.OrdinalIgnoreCase)
            || key.Equals("title", StringComparison.OrdinalIgnoreCase)
            || key.Equals("status", StringComparison.OrdinalIgnoreCase)
            || key.Equals("extensions", StringComparison.OrdinalIgnoreCase);
    }
}
