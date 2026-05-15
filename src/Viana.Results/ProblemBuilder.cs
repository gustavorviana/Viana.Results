using System;
using System.Collections.Generic;

namespace Viana.Results;

/// <summary>
/// Fluent builder for <see cref="ProblemResult"/>. Useful when the problem details
/// payload is composed across multiple steps (e.g. handler validation + cross-cutting
/// pipeline behavior adding extensions). Call <see cref="Build"/> to materialize the
/// immutable <see cref="ProblemResult"/>.
/// </summary>
public sealed class ProblemBuilder
{
    private int _status;
    private string? _title;
    private string? _type;
    private string? _description;
    private readonly Dictionary<string, object?> _extensions = new(StringComparer.Ordinal);

    /// <summary>Initializes a new builder for the given HTTP status code.</summary>
    /// <param name="status">RFC 9457 HTTP status code (e.g. 400, 404, 422, 500).</param>
    public ProblemBuilder(int status)
    {
        _status = status;
    }

    /// <summary>Replaces the HTTP status code.</summary>
    public ProblemBuilder WithStatus(int status)
    {
        _status = status;
        return this;
    }

    /// <summary>Sets the RFC 9457 short, human-readable title.</summary>
    public ProblemBuilder WithTitle(string? title)
    {
        _title = title;
        return this;
    }

    /// <summary>Sets the RFC 9457 problem type URI. Defaults to "about:blank" when null/empty.</summary>
    public ProblemBuilder WithType(string? type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the optional human-readable description. Stored under the "description" key
    /// in <see cref="ProblemResult.Extensions"/>.
    /// </summary>
    public ProblemBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the RFC 9457 "detail" member — a human-readable explanation specific to this
    /// occurrence of the problem. Stored under the "detail" key in <see cref="ProblemResult.Extensions"/>.
    /// </summary>
    public ProblemBuilder WithDetail(string? detail)
    {
        if (detail == null)
            _extensions.Remove("detail");
        else
            _extensions["detail"] = detail;
        return this;
    }

    /// <summary>
    /// Sets the RFC 9457 "instance" member — a URI reference identifying the specific occurrence
    /// of the problem (e.g. the request path or a correlation URI).
    /// Stored under the "instance" key in <see cref="ProblemResult.Extensions"/>.
    /// </summary>
    public ProblemBuilder WithInstance(string? instance)
    {
        if (instance == null)
            _extensions.Remove("instance");
        else
            _extensions["instance"] = instance;
        return this;
    }

    /// <summary>
    /// Adds (or overwrites) a single extension member. Reserved RFC 9457 members
    /// ("type", "title", "status", "extensions") are filtered out during <see cref="Build"/>.
    /// </summary>
    public ProblemBuilder AddExtension(string key, object? value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Extension key must be non-empty.", nameof(key));

        _extensions[key] = value;
        return this;
    }

    /// <summary>Adds (or overwrites) multiple extension members at once.</summary>
    public ProblemBuilder AddExtensions(IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        if (extensions == null) throw new ArgumentNullException(nameof(extensions));

        foreach (var kv in extensions)
        {
            if (string.IsNullOrEmpty(kv.Key))
                continue;
            _extensions[kv.Key] = kv.Value;
        }
        return this;
    }

    /// <summary>Builds the immutable <see cref="ProblemResult"/> from the accumulated state.</summary>
    public ProblemResult Build()
    {
        return ProblemResult.WithDescription(
            status: _status,
            title: _title ?? string.Empty,
            description: _description,
            type: _type ?? "about:blank",
            extensions: _extensions.Count > 0 ? _extensions : null);
    }
}
