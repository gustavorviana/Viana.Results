using System;
using System.Collections.Generic;
using System.Net;

namespace Viana.Results;

/// <summary>
/// Centralizes default messages used by <see cref="Results"/> factory methods.
/// Uses a dictionary as the single source of truth; properties are quick-access to the same entries.
/// Add any status code via <see cref="_messages"/> or <see cref="Register"/>.
/// </summary>
public static class ResultMessages
{
    /// <summary>
    /// Key for validation-failure message (HTTP 400 variant). Use when you need a different default for validation vs generic bad request.
    /// </summary>
    public const int Key400Validation = -400;

    /// <summary>
    /// Dictionary of status codes to default titles. All codes used by <see cref="Results"/> are
    /// pre-filled. Use <see cref="Key400Validation"/> for the validation (400) variant.
    /// </summary>
    private static readonly Dictionary<int, string> _messages = new()
    {
        [200] = "Ok",
        [400] = "Bad Request",
        [Key400Validation] = "Validation Failed",
        [401] = "Unauthorized Access.",
        [403] = "Forbidden Access.",
        [404] = "The Requested Resource Was Not Found.",
        [409] = "Conflict Occurred.",
        [422] = "Unprocessable Entity"
    };

    /// <summary>Registers or overrides the default message for a status code.</summary>
    /// <param name="statusCode">HTTP status code (e.g. 418, 503).</param>
    /// <param name="message">Default title for that code.</param>
    public static void Register(int statusCode, string message) => _messages[statusCode] = message;

    /// <summary>Gets the default message for a status code, if registered.</summary>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="message">When found, the registered message.</param>
    /// <returns>True if the code was registered.</returns>
    public static bool TryGet(int statusCode, out string message)
    {
        if (_messages.TryGetValue(statusCode, out var foundMessage))
        {
            message = foundMessage ?? string.Empty;
            return true;
        }

        message = AddSpacesToPascalCase(statusCode);
        return !string.IsNullOrEmpty(message);
    }

    /// <summary>Default title for success (HTTP 200). Maps to <see cref="_messages"/>[200].</summary>
    public static string Ok
    {
        get => TryGet(200, out var m) ? m : "Ok";
        set => _messages[200] = value;
    }

    /// <summary>Default title for bad request (HTTP 400). Maps to <see cref="_messages"/>[400].</summary>
    public static string BadRequest
    {
        get => TryGet(400, out var m) ? m : "Bad Request";
        set => _messages[400] = value;
    }

    /// <summary>Default title when resource was not found (HTTP 404). Maps to <see cref="_messages"/>[404].</summary>
    public static string NotFound
    {
        get => TryGet(404, out var m) ? m : "The Requested Resource Was Not Found.";
        set => _messages[404] = value;
    }

    /// <summary>Default title for unauthorized (HTTP 401). Maps to <see cref="_messages"/>[401].</summary>
    public static string Unauthorized
    {
        get => TryGet(401, out var m) ? m : "Unauthorized Access.";
        set => _messages[401] = value;
    }

    /// <summary>Default title for forbidden (HTTP 403). Maps to <see cref="_messages"/>[403].</summary>
    public static string Forbidden
    {
        get => TryGet(403, out var m) ? m : "Forbidden Access.";
        set => _messages[403] = value;
    }

    /// <summary>Default title for conflict (HTTP 409). Maps to <see cref="_messages"/>[409].</summary>
    public static string Conflict
    {
        get => TryGet(409, out var m) ? m : "Conflict Occurred.";
        set => _messages[409] = value;
    }

    /// <summary>Default title for validation failure (HTTP 400). Maps to <see cref="_messages"/>[<see cref="Key400Validation"/>].</summary>
    public static string ValidationFailed
    {
        get => TryGet(Key400Validation, out var m) ? m : "Validation Failed";
        set => _messages[Key400Validation] = value;
    }

    /// <summary>Default title for business rule / unprocessable (HTTP 422). Maps to <see cref="_messages"/>[422].</summary>
    public static string UnprocessableEntity
    {
        get => TryGet(422, out var m) ? m : "Unprocessable Entity";
        set => _messages[422] = value;
    }

    private static string AddSpacesToPascalCase(int code)
    {
        if (!Enum.IsDefined(typeof(HttpStatusCode), code))
            return string.Empty;

        var input = code.ToString();
        var sb = new System.Text.StringBuilder(input.Length + 10);

        sb.Append(input[0]);

        for (int i = 1; i < input.Length; i++)
        {
            var c = input[i];

            if (char.IsUpper(c) && !char.IsWhiteSpace(input[i - 1]))
                sb.Append(' ');

            sb.Append(c);
        }

        return sb.ToString();
    }
}
