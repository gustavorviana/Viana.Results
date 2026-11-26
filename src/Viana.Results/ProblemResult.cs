using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Viana.Results;

/// <summary>
/// Represents an RFC 9457 "Problem Details" response payload for HTTP APIs.
///
/// Spec: RFC 9457 - Problem Details for HTTP APIs
/// https://datatracker.ietf.org/doc/html/rfc9457
/// </summary>
[JsonConverter(typeof(ProblemResultJsonConverter))]
public sealed class ProblemResult: IHasExtensions
{
    /// <summary>RFC 9457: problem type URI.</summary>
    public string Type { get; }

    /// <summary>RFC 9457: short, human-readable summary of the problem type. Defaults to the status reason phrase (or "Error" for unknown statuses) when not provided.</summary>
    public string Title { get; }

    /// <summary>RFC 9457: HTTP status code.</summary>
    public int Status { get; }

    /// <summary>RFC 9457 extension members. The RFC 9457 "detail" member is stored under key "detail" (see <see cref="WithDetail"/>); the legacy "description" under key "description".</summary>
    public IReadOnlyDictionary<string, object?> Extensions { get; }

    /// <summary>
    /// Initializes a new instance with the specified status, title, type and extensions (no description).
    /// </summary>
    /// <param name="status">RFC 9457: HTTP status code.</param>
    /// <param name="title">RFC 9457: short, human-readable summary of the problem type. When null, empty or whitespace, defaults to the reason phrase of <paramref name="status"/> (e.g. 404 "Not Found"), or "Error" for unknown statuses.</param>
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

    private ProblemResult(
        int status,
        string title,
        string? description,
        string type,
        IReadOnlyDictionary<string, object?>? extensions)
    {
        Status = status;
        Type = string.IsNullOrWhiteSpace(type) ? "about:blank" : type;
        Title = string.IsNullOrWhiteSpace(title) ? DefaultTitle(status) : title;
        Extensions = NormalizeExtensions(extensions, description);
    }

    private const string FallbackTitle = "Error";

    // RFC 9457 §4.2.1 ("about:blank") / §3.1.3 ("title"): a missing title defaults to the status reason phrase; unknown statuses use "Error".
    private static string DefaultTitle(int status) =>
        HttpStatusInfo.TryGetReasonPhrase(status, out var phrase) ? phrase : FallbackTitle;

    /// <summary>
    /// Creates a new instance. When <paramref name="description"/> is provided, it is stored in <see cref="Extensions"/> under key "description".
    /// </summary>
    /// <param name="status">RFC 9457: HTTP status code.</param>
    /// <param name="title">RFC 9457: short, human-readable summary of the problem type. When null, empty or whitespace, defaults to the reason phrase of <paramref name="status"/> (e.g. 404 "Not Found"), or "Error" for unknown statuses.</param>
    /// <param name="description">Optional. RFC 9457: human-readable explanation. Stored in <see cref="Extensions"/> when provided.</param>
    /// <param name="type">RFC 9457: problem type URI. Defaults to "about:blank".</param>
    /// <param name="extensions">RFC 9457: optional extension members.</param>
    /// <returns>A new <see cref="ProblemResult"/> instance.</returns>
    [Obsolete("\"description\" is not an RFC 9457 member. Use ProblemResult.WithDetail (RFC 9457 \"detail\") instead.")]
    public static ProblemResult WithDescription(
        int status,
        string title,
        string? description = null,
        string type = "about:blank",
        IReadOnlyDictionary<string, object?>? extensions = null)
        => CreateWithDescription(status, title, description, type, extensions);

    /// <summary>
    /// Creates a new instance with the RFC 9457 "detail" member (§3.1.4): a human-readable explanation
    /// specific to this occurrence of the problem. Stored in <see cref="Extensions"/> under key "detail".
    /// </summary>
    /// <param name="status">RFC 9457: HTTP status code.</param>
    /// <param name="title">RFC 9457: short, human-readable summary of the problem type. When null, empty or whitespace, defaults to the reason phrase of <paramref name="status"/>, or "Error" for unknown statuses.</param>
    /// <param name="detail">Optional. RFC 9457 "detail". Not stored when null or empty; when provided it overrides an "detail" entry in <paramref name="extensions"/>.</param>
    /// <param name="type">RFC 9457: problem type URI. Defaults to "about:blank".</param>
    /// <param name="extensions">RFC 9457: optional extension members.</param>
    /// <returns>A new <see cref="ProblemResult"/> instance.</returns>
    public static ProblemResult WithDetail(
        int status,
        string title,
        string? detail = null,
        string type = "about:blank",
        IReadOnlyDictionary<string, object?>? extensions = null)
    {
        if (string.IsNullOrEmpty(detail))
            return new ProblemResult(status, title, type, extensions);

        var copy = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (extensions != null)
        {
            foreach (var kv in extensions)
                copy[kv.Key] = kv.Value;
        }

        copy["detail"] = detail;
        return new ProblemResult(status, title, type, copy);
    }

    // Non-obsolete path for the legacy "description" member, used by ProblemBuilder and JSON reading.
    internal static ProblemResult CreateWithDescription(
        int status,
        string title,
        string? description,
        string type,
        IReadOnlyDictionary<string, object?>? extensions)
    {
        if (string.IsNullOrEmpty(description))
            return new ProblemResult(status, title, type, extensions);
        return new ProblemResult(status, title, description, type, extensions);
    }

    private static IReadOnlyDictionary<string, object?> NormalizeExtensions(IReadOnlyDictionary<string, object?>? extensions, string? description)
    {
        var copy = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (extensions != null)
        {
            foreach (var kv in extensions)
            {
                if (IsStandardMember(kv.Key))
                    continue;

                copy[kv.Key] = kv.Value; // case-insensitive: last value wins, first casing kept
            }
        }

        if (!string.IsNullOrEmpty(description))
            copy["description"] = description;

        if (copy.Count == 0)
            return EmptyExtensions;

        return new ReadOnlyDictionary<string, object?>(copy);
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyExtensions =
        new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase));

    private static bool IsStandardMember(string key)
    {
        return key.Equals("type", StringComparison.OrdinalIgnoreCase)
            || key.Equals("title", StringComparison.OrdinalIgnoreCase)
            || key.Equals("status", StringComparison.OrdinalIgnoreCase)
            || key.Equals("extensions", StringComparison.OrdinalIgnoreCase);
    }
}
