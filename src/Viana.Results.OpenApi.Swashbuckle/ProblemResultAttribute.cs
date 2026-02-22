using System;

namespace Viana.Results.OpenApi.Swashbuckle;

/// <summary>
/// Documents one problem response for Swagger with the same options as <see cref="ProblemResult"/>:
/// status, title, type (RFC 9457 problem type URI), and optional description.
/// Apply multiple attributes to document multiple error responses for the same action.
/// </summary>
/// <remarks>
/// Example: <c>[<see cref="ProblemResultAttribute"/>(404, Title = "User not found")]</c>,
/// <c>[<see cref="ProblemResultAttribute"/>>(400, Title = "Validation failed", Description = "One or more fields are invalid.")]</c>
/// </remarks>
/// <remarks>
/// Initializes the attribute for one error response.
/// </remarks>
/// <param name="status">HTTP status code (e.g. 400, 404, 422).</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class ProblemResultAttribute(int status) : Attribute
{
    /// <summary>
    /// HTTP status code for this error (e.g. 400, 404).
    /// </summary>
    public int Status { get; } = status;

    /// <summary>
    /// RFC 9457 short, human-readable summary. When null, a default is used from <see cref="Viana.Results.ResultMessages"/> when available.
    /// </summary>
    public string? Title { get; set; }

	/// <summary>
	/// RFC 9457 problem type URI. Defaults to "about:blank" when null or empty.
	/// </summary>
	public string? Type { get; set; }

	/// <summary>
	/// Optional human-readable explanation; stored in RFC 9457 extensions under key "description".
	/// </summary>
	public string? Description { get; set; }
}