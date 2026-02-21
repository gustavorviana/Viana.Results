using System;
using System.Collections.Generic;
using System.Net;

namespace Viana.Results;

/// <summary>
/// Provides factory methods for creating result instances.
/// </summary>
public static class Results
{
    /// <summary>
    /// Creates a successful result without payload (status 200, no problem).
    /// </summary>
    /// <returns>A successful result with no data.</returns>
    public static Result Ok()
    {
        return new Result(200);
    }

    /// <summary>
    /// Creates a successful result with the given message as data.
    /// </summary>
    /// <param name="message">The success message used as result data.</param>
    /// <returns>A successful result containing the message as data.</returns>
    public static Result<string> Ok(string message)
    {
        return new Result<string>(message, 200);
    }

    /// <summary>
    /// Creates a successful result with the given data.
    /// </summary>
    /// <param name="data">The result payload.</param>
    /// <returns>A successful result containing the data.</returns>
    public static Result<TValue> Ok<TValue>(TValue data)
    {
        return new Result<TValue>(data, 200);
    }

    /// <summary>
    /// Creates a result indicating a resource was created (HTTP 201) with the given body.
    /// </summary>
    /// <param name="data">The created resource payload.</param>
    /// <returns>A result with status 201 and the given data.</returns>
    public static Result<TValue> Created<TValue>(TValue data)
    {
        return new Result<TValue>(data, 201);
    }

    /// <summary>
    /// Creates a result indicating success with no response body (HTTP 204 No Content).
    /// </summary>
    /// <returns>A result with status 204 and no data.</returns>
    public static Result NoContent()
    {
        return new Result(204);
    }

    /// <summary>
    /// Creates a failure result with the given HTTP status code and title.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="title">The problem title (RFC 9457). When null, resolved from <see cref="ResultMessages"/> by status code.</param>
    /// <returns>A failure result with the given status and problem title.</returns>
    public static Result Failure(HttpStatusCode statusCode, string? title = null)
    {
        return Failure((int)statusCode, title);
    }

    /// <summary>
    /// Creates a failure result with the given HTTP status code (integer) and title.
    /// </summary>
    /// <param name="status">The HTTP status code (e.g. 400, 404, 422, 500).</param>
    /// <param name="title">The problem title (RFC 9457). When null, resolved from <see cref="ResultMessages"/> by status code.</param>
    /// <returns>A failure result with the given status and problem title.</returns>
    public static Result Failure(int status, string? title = null)
    {
        return new Result(status, new ProblemResult(status, GetErrorMessage(status, title)));
    }

    /// <summary>
    /// Creates a failure result from an exception, using its message as the problem title.
    /// </summary>
    /// <param name="exception">The exception; its message is used as the problem title.</param>
    /// <param name="statusCode">The HTTP status code. Defaults to 500 (Internal Server Error).</param>
    /// <returns>A failure result with the exception message as title and the given status.</returns>
    public static Result Failure(Exception exception, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        var status = (int)statusCode;
        return new Result(status, new ProblemResult(status, GetErrorMessage(status, exception.Message)));
    }

    private static string GetErrorMessage(int status, string? title)
    {
        if (!string.IsNullOrEmpty(title))
            return title!;

        if (ResultMessages.TryGet(status, out var m) && !string.IsNullOrEmpty(m))
            return m!;

        if (status == 500)
            return "Internal Server Error";

        return "Undefined Error";
    }

    /// <summary>
    /// Creates a result indicating a bad request (HTTP 400) with the default title.
    /// </summary>
    /// <returns>A result with status 400 and title from <see cref="ResultMessages.BadRequest"/>.</returns>
    public static Result BadRequest()
    {
        return new Result(400, new ProblemResult(400, ResultMessages.BadRequest));
    }

    /// <summary>
    /// Creates a result indicating a bad request (HTTP 400) with the given title.
    /// </summary>
    /// <param name="title">The problem title. When null, uses <see cref="ResultMessages"/>[400].</param>
    /// <returns>A result with status 400 and the given title.</returns>
    public static Result BadRequest(string? title)
    {
        return new Result(400, new ProblemResult(400, title ?? ResultMessages.BadRequest));
    }

    /// <summary>
    /// Creates a result indicating a business rule violation (HTTP 422) with the given title.
    /// </summary>
    /// <param name="message">The business rule violation title. When null, uses <see cref="ResultMessages"/>[422].</param>
    /// <returns>A result with status 422 and the given title.</returns>
    public static Result BusinessRuleViolated(string? message)
    {
        return new Result(422, new ProblemResult(422, message ?? ResultMessages.UnprocessableEntity));
    }

    /// <summary>
    /// Creates a result indicating that the requested resource was not found (HTTP 404) with the default title.
    /// </summary>
    /// <returns>A result with status 404 and title from <see cref="ResultMessages.NotFound"/>.</returns>
    public static Result NotFound()
    {
        return new Result(404, new ProblemResult(404, ResultMessages.NotFound));
    }

    /// <summary>
    /// Creates a result indicating that the requested resource was not found (HTTP 404) with the given title.
    /// </summary>
    /// <param name="title">The problem title. When null, uses <see cref="ResultMessages"/>[404].</param>
    /// <returns>A result with status 404 and the given title.</returns>
    public static Result NotFound(string? title)
    {
        return new Result(404, new ProblemResult(404, title ?? ResultMessages.NotFound));
    }

    /// <summary>
    /// Creates a result indicating unauthorized access (HTTP 401) with the default title.
    /// </summary>
    /// <returns>A result with status 401 and title from <see cref="ResultMessages.Unauthorized"/>.</returns>
    public static Result Unauthorized()
    {
        return new Result(401, new ProblemResult(401, ResultMessages.Unauthorized));
    }

    /// <summary>
    /// Creates a result indicating unauthorized access (HTTP 401) with the given title.
    /// </summary>
    /// <param name="title">The problem title. When null, uses <see cref="ResultMessages"/>[401].</param>
    /// <returns>A result with status 401 and the given title.</returns>
    public static Result Unauthorized(string? title)
    {
        return new Result(401, new ProblemResult(401, title ?? ResultMessages.Unauthorized));
    }

    /// <summary>
    /// Creates a result indicating forbidden access (HTTP 403) with the default title.
    /// </summary>
    /// <returns>A result with status 403 and title from <see cref="ResultMessages.Forbidden"/>.</returns>
    public static Result Forbidden()
    {
        return new Result(403, new ProblemResult(403, ResultMessages.Forbidden));
    }

    /// <summary>
    /// Creates a result indicating forbidden access (HTTP 403) with the given title.
    /// </summary>
    /// <param name="title">The problem title. When null, uses <see cref="ResultMessages"/>[403].</param>
    /// <returns>A result with status 403 and the given title.</returns>
    public static Result Forbidden(string? title)
    {
        return new Result(403, new ProblemResult(403, title ?? ResultMessages.Forbidden));
    }

    /// <summary>
    /// Creates a result indicating a conflict (HTTP 409) with the default title.
    /// </summary>
    /// <returns>A result with status 409 and title from <see cref="ResultMessages.Conflict"/>.</returns>
    public static Result Conflict()
    {
        return new Result(409, new ProblemResult(409, ResultMessages.Conflict));
    }

    /// <summary>
    /// Creates a result indicating a conflict (HTTP 409) with the given title.
    /// </summary>
    /// <param name="title">The problem title. When null, uses <see cref="ResultMessages"/>[409].</param>
    /// <returns>A result with status 409 and the given title.</returns>
    public static Result Conflict(string? title)
    {
        return new Result(409, new ProblemResult(409, title ?? ResultMessages.Conflict));
    }

    /// <summary>
    /// Creates a result indicating validation failure (HTTP 400) with detailed errors in problem extensions.
    /// </summary>
    /// <param name="errors">Field names as keys and arrays of validation error messages as values.</param>
    /// <param name="title">The problem title; when null, uses <see cref="ResultMessages.ValidationFailed"/>.</param>
    /// <returns>A result with status 400, the given title, and errors in <see cref="ProblemResult.Extensions"/> under key "errors".</returns>
    public static Result Validation(Dictionary<string, string[]> errors, string? title = null)
    {
        var problemTitle = title ?? ResultMessages.ValidationFailed;
        var extensions = errors is null or { Count: 0 }
            ? null
            : new Dictionary<string, object?>(StringComparer.Ordinal) { ["errors"] = errors };
        return new Result(400, new ProblemResult(400, problemTitle, "about:blank", extensions));
    }

    /// <summary>
    /// Creates a result indicating validation failure (HTTP 400) with detailed errors in problem extensions.
    /// </summary>
    /// <param name="errors">Field names as keys and lists of validation error messages as values.</param>
    /// <param name="title">The problem title; when null, uses <see cref="ResultMessages.ValidationFailed"/>.</param>
    /// <returns>A result with status 400, the given title, and errors in <see cref="ProblemResult.Extensions"/> under key "errors".</returns>
    public static Result Validation(Dictionary<string, List<string>> errors, string? title = null)
    {
        var problemTitle = title ?? ResultMessages.ValidationFailed;
        var extensions = errors is null or { Count: 0 }
            ? null
            : new Dictionary<string, object?>(StringComparer.Ordinal) { ["errors"] = errors };
        return new Result(400, new ProblemResult(400, problemTitle, "about:blank", extensions));
    }
}
