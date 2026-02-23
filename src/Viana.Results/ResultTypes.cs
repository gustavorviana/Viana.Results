using System;
using System.Collections.Concurrent;

namespace Viana.Results
{
    /// <summary>
    /// Centralizes default "type" URIs used by the Results factory methods (RFC 9457 / Problem Details).
    /// Uses a dictionary as the single source of truth; properties are quick-access to the same entries.
    /// If a type is not registered for a given status code, it returns "about:blank" by default.
    /// </summary>
    public static class ResultTypes
    {
        /// <summary>
        /// Key for validation-failure type (HTTP 400 variant). Use when you need a different default for validation vs generic bad request.
        /// </summary>
        public const int Key400Validation = -400;

        /// <summary>
        /// Default fallback type when a status code has no registered mapping (RFC 9457).
        /// </summary>
        public const string AboutBlank = "about:blank";

        private static readonly ConcurrentDictionary<int, string> _types = new();

        /// <summary>
        /// Registers or overrides the default type for a status code.
        /// </summary>
        /// <param name="statusCode">HTTP status code (e.g. 418, 503).</param>
        /// <param name="type">Type URI (absolute URI or "about:blank").</param>
        public static void Register(int statusCode, string type)
        {
            _types[statusCode] = NormalizeType(type);
        }

        /// <summary>
        /// Tries to get the registered type for a status code (or returns "about:blank" if missing).
        /// </summary>
        public static bool TryGet(int statusCode, out string type)
        {
            if (_types.TryGetValue(statusCode, out var found))
            {
                type = string.IsNullOrWhiteSpace(found) ? AboutBlank : found;
                return true;
            }

            type = AboutBlank;
            return false;
        }

        /// <summary>
        /// Gets the default type for a status code, falling back to "about:blank" when missing.
        /// </summary>
        public static string GetOrDefault(int statusCode)
        {
            return _types.TryGetValue(statusCode, out var found) && !string.IsNullOrWhiteSpace(found)
                ? found
                : AboutBlank;
        }

        /// <summary>
        /// Gets a "canonical" default type for a code when you want deterministic URIs.
        /// If the code isn't a valid HttpStatusCode name, returns "about:blank".
        /// Example: 404 -> "https://httpstatuses.com/404" (if you pick that base).
        /// </summary>
        public static string GetCanonicalOrAboutBlank(int statusCode, string baseUri)
        {
            if (string.IsNullOrWhiteSpace(baseUri))
                return AboutBlank;

            // baseUri like "https://httpstatuses.com/"
            baseUri = baseUri.TrimEnd('/') + "/";

            return baseUri + statusCode.ToString();
        }

        private static string NormalizeType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return AboutBlank;

            type = type!.Trim();

            // Allow about:blank explicitly
            if (string.Equals(type, AboutBlank, StringComparison.OrdinalIgnoreCase))
                return AboutBlank;

            // Accept absolute URIs; otherwise fallback to about:blank (RFC 9457 expects a URI)
            if (Uri.TryCreate(type, UriKind.Absolute, out _))
                return type;

            return AboutBlank;
        }
    }
}