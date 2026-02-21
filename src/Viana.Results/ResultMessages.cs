using System.Collections.Generic;

namespace Viana.Results;

/// <summary>
/// Centralizes default messages used by <see cref="Results"/> factory methods.
/// Uses a dictionary as the single source of truth; properties are quick-access to the same entries.
/// Add any status code via <see cref="Messages"/> or <see cref="Register"/>.
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
    public static IDictionary<int, string> Messages { get; } = new Dictionary<int, string>
    {
        [200] = "Ok",
        [400] = "Bad request",
        [Key400Validation] = "Validation failed",
        [401] = "Unauthorized access.",
        [403] = "Forbidden access.",
        [404] = "The requested resource was not found.",
        [409] = "Conflict occurred.",
        [422] = "Unprocessable Entity"
    };

    /// <summary>Registers or overrides the default message for a status code.</summary>
    /// <param name="statusCode">HTTP status code (e.g. 418, 503).</param>
    /// <param name="message">Default title for that code.</param>
    public static void Register(int statusCode, string message) => Messages[statusCode] = message;

    /// <summary>Gets the default message for a status code, if registered.</summary>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="message">When found, the registered message.</param>
    /// <returns>True if the code was registered.</returns>
    public static bool TryGet(int statusCode, out string? message) => Messages.TryGetValue(statusCode, out message);

    /// <summary>Default title for success (HTTP 200). Maps to <see cref="Messages"/>[200].</summary>
    public static string Ok
    {
        get => Messages.TryGetValue(200, out var m) ? m : "Ok";
        set => Messages[200] = value;
    }

    /// <summary>Default title for bad request (HTTP 400). Maps to <see cref="Messages"/>[400].</summary>
    public static string BadRequest
    {
        get => Messages.TryGetValue(400, out var m) ? m : "Bad request";
        set => Messages[400] = value;
    }

    /// <summary>Default title when resource was not found (HTTP 404). Maps to <see cref="Messages"/>[404].</summary>
    public static string NotFound
    {
        get => Messages.TryGetValue(404, out var m) ? m : "The requested resource was not found.";
        set => Messages[404] = value;
    }

    /// <summary>Default title for unauthorized (HTTP 401). Maps to <see cref="Messages"/>[401].</summary>
    public static string Unauthorized
    {
        get => Messages.TryGetValue(401, out var m) ? m : "Unauthorized access.";
        set => Messages[401] = value;
    }

    /// <summary>Default title for forbidden (HTTP 403). Maps to <see cref="Messages"/>[403].</summary>
    public static string Forbidden
    {
        get => Messages.TryGetValue(403, out var m) ? m : "Forbidden access.";
        set => Messages[403] = value;
    }

    /// <summary>Default title for conflict (HTTP 409). Maps to <see cref="Messages"/>[409].</summary>
    public static string Conflict
    {
        get => Messages.TryGetValue(409, out var m) ? m : "Conflict occurred.";
        set => Messages[409] = value;
    }

    /// <summary>Default title for validation failure (HTTP 400). Maps to <see cref="Messages"/>[<see cref="Key400Validation"/>].</summary>
    public static string ValidationFailed
    {
        get => Messages.TryGetValue(Key400Validation, out var m) ? m : "Validation failed";
        set => Messages[Key400Validation] = value;
    }

    /// <summary>Default title for business rule / unprocessable (HTTP 422). Maps to <see cref="Messages"/>[422].</summary>
    public static string UnprocessableEntity
    {
        get => Messages.TryGetValue(422, out var m) ? m : "Unprocessable Entity";
        set => Messages[422] = value;
    }
}
