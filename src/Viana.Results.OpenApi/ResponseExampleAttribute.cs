using System;

namespace Viana.Results.OpenApi;

/// <summary>
/// Associates an <see cref="IExampleProvider{T}"/> with a specific response status code
/// for OpenAPI/Swagger documentation. Multiple attributes can be applied to the same
/// method (or controller) to document different statuses or named example variants.
/// </summary>
/// <remarks>
/// Precedence (most specific wins): method attribute > class attribute > global registration.
/// When a method-level example is found for a status code, class-level and global examples
/// for the same status are ignored for that action.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class ResponseExampleAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance pairing a status code with an <see cref="IExampleProvider{T}"/> type.
    /// </summary>
    /// <param name="statusCode">The HTTP status code (e.g. 200, 400, 404, 500).</param>
    /// <param name="providerType">A type implementing <see cref="IExampleProvider{T}"/>.</param>
    public ResponseExampleAttribute(int statusCode, Type providerType)
    {
        if (providerType == null)
            throw new ArgumentNullException(nameof(providerType));

        StatusCode = statusCode;
        ProviderType = providerType;
    }

    /// <summary>HTTP status code this example is associated with.</summary>
    public int StatusCode { get; }

    /// <summary>The provider type implementing <see cref="IExampleProvider{T}"/>.</summary>
    public Type ProviderType { get; }

    /// <summary>
    /// Optional name when multiple examples are attached to the same status code.
    /// When set, the example is emitted under <c>responses[status].content[type].examples[Name]</c>
    /// (OpenAPI 3 multi-example map). When omitted (and only one example exists for the status),
    /// the example is emitted as the singular <c>example</c>.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>Optional short summary shown next to the example in the OpenAPI UI.</summary>
    public string? Summary { get; set; }

    /// <summary>Optional longer description for the example.</summary>
    public string? Description { get; set; }
}
