using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Viana.Results.OpenApi.Schemas;

/// <summary>
/// Represents an OpenAPI schema builder for RFC 9457 Problem Details responses.
/// Produces standardized OpenAPI documentation including default and custom fields.
/// </summary>
public class ProblemResultSchema
{
    private readonly List<ProblemField> _schemas = [];
    private readonly JsonSerializerOptions _options;
    private readonly string _title;

    /// <summary>
    /// Gets the HTTP status code associated with this problem schema.
    /// </summary>
    public int Status { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ProblemResultSchema"/> from an existing <see cref="ProblemResult"/>.
    /// Extension fields from the result are automatically added to the schema.
    /// </summary>
    /// <param name="options">Serializer options used for type inference.</param>
    /// <param name="result">The problem result instance.</param>
    public ProblemResultSchema(JsonSerializerOptions options, ProblemResult result) : this(options, result.Status, result.Type, result.Title)
    {
        foreach (var extension in result.Extensions)
            AddFieldCore(GetType(extension.Value?.GetType()), extension.Key, "", extension.Value);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ProblemResultSchema"/> using explicit values.
    /// Standard RFC 9457 fields (<c>status</c>, <c>type</c>, <c>title</c>) are automatically added.
    /// </summary>
    /// <param name="options">Serializer options used for type inference.</param>
    /// <param name="status">HTTP status code.</param>
    /// <param name="type">Problem type URI. If null, a default is resolved.</param>
    /// <param name="title">Human-readable title. If null, a default is resolved.</param>
    public ProblemResultSchema(JsonSerializerOptions options, int status, string? type = null, string? title = null)
    {
        _options = options;
        _title = string.IsNullOrEmpty(title) ? GetDefaultDescription(status) : title;
        if (string.IsNullOrEmpty(type))
            type = ResultTypes.GetOrDefault(status);

        Status = status;

        _schemas.AddRange([
            new ProblemField(JsonSchemaType.Integer, "status", "RFC 9457: HTTP status code", status),
            new ProblemField(JsonSchemaType.String, "type", "RFC 9457: problem type URI", type),
            new ProblemField(JsonSchemaType.String, "title", "RFC 9457: short, human-readable summary", _title)
        ]);
    }

    /// <summary>
    /// Creates a schema instance from a <see cref="ProblemResultAttribute"/>.
    /// </summary>
    /// <param name="options">Serializer options used for type inference.</param>
    /// <param name="attribute">Attribute containing problem metadata.</param>
    /// <returns>A configured <see cref="ProblemResultSchema"/>.</returns>
    public static ProblemResultSchema FromAttribute(JsonSerializerOptions options, ProblemResultAttribute attribute)
    {
        var schema = new ProblemResultSchema(options, attribute.Status, attribute.Type, attribute.Title);
        if (!string.IsNullOrEmpty(attribute.Detail))
            schema.AddFieldCore(JsonSchemaType.String, "detail", "RFC 9457: human-readable explanation specific to this occurrence", attribute.Detail);
        return schema;
    }

    /// <summary>
    /// Adds a custom field definition to the problem schema.
    /// </summary>
    /// <param name="field">Field schema definition.</param>
    /// <returns>The current instance for fluent chaining.</returns>
    [Obsolete("Internal helper; it will become internal in Viana.Results.OpenApi 2.0.0.")]
    public ProblemResultSchema AddField(ProblemFieldSchema field)
    {
        return AddFieldCore(field.Type, field.Name, field.Description, field.ExampleValue);
    }

    /// <summary>
    /// Adds a custom field definition using a JSON schema type.
    /// </summary>
    /// <param name="type">JSON schema type.</param>
    /// <param name="name">Field name.</param>
    /// <param name="description">Field description.</param>
    /// <param name="exampleValue">Optional example value.</param>
    /// <returns>The current instance for fluent chaining.</returns>
    [Obsolete("Internal helper; it will become internal in Viana.Results.OpenApi 2.0.0.")]
    public ProblemResultSchema AddField(JsonSchemaType type, string name, string description, object? exampleValue = null)
        => AddFieldCore(type, name, description, exampleValue);

    // Names are case-insensitive like the runtime (I10): an existing name is updated in place,
    // keeping the first casing; the last type/description/example wins.
    internal ProblemResultSchema AddFieldCore(JsonSchemaType type, string name, string description, object? exampleValue = null)
    {
        // Extension names follow the application naming policy, like the runtime (FR-018); RFC members are fixed.
        if (!IsBaseMember(name))
            name = _options.PropertyNamingPolicy?.ConvertName(name) ?? name;

        var index = _schemas.FindIndex(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
            _schemas[index] = new ProblemField(type, _schemas[index].Name, description, exampleValue);
        else
            _schemas.Add(new ProblemField(type, name, description, exampleValue));
        return this;
    }

    /// <summary>
    /// Adds a custom field definition using a CLR type that will be mapped to a JSON schema type.
    /// </summary>
    /// <param name="type">CLR type.</param>
    /// <param name="name">Field name.</param>
    /// <param name="description">Field description.</param>
    /// <param name="exampleValue">Optional example value.</param>
    /// <returns>The current instance for fluent chaining.</returns>
    [Obsolete("Internal helper; it will become internal in Viana.Results.OpenApi 2.0.0.")]
    public ProblemResultSchema AddField(Type? type, string name, string description, object? exampleValue = null)
        => AddFieldCore(type, name, description, exampleValue);

    internal ProblemResultSchema AddFieldCore(Type? type, string name, string description, object? exampleValue = null)
        => AddFieldCore(GetType(type), name, description, exampleValue);

    private JsonSchemaType GetType(Type? type)
    {
        if (type == null)
            return JsonSchemaType.Null;

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(bool))
            return JsonSchemaType.Boolean;

        if (type.IsEnum)
            return IsStringEnum(type) ? JsonSchemaType.String : JsonSchemaType.Number;

        if (type == typeof(byte) ||
            type == typeof(sbyte) ||
            type == typeof(short) ||
            type == typeof(ushort) ||
            type == typeof(int) ||
            type == typeof(uint) ||
            type == typeof(long) ||
            type == typeof(ulong))
            return JsonSchemaType.Integer;

        if (type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal))
            return JsonSchemaType.Number;

        if (type == typeof(string) ||
            type == typeof(char) ||
            type == typeof(Guid) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type == typeof(Uri))
            return JsonSchemaType.String;

        if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            return JsonSchemaType.Array;

        return JsonSchemaType.Object;
    }

    private bool IsStringEnum(Type type)
    {
        return !IsEnumToInt(_options.Converters.FirstOrDefault(x => x.CanConvert(type))?.GetType()) &&
            !IsEnumToInt(type.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType);
    }

    private static bool IsEnumToInt(Type? converterType)
    {
        return converterType != null && converterType.IsGenericType && converterType.GetGenericTypeDefinition() == typeof(JsonNumberEnumConverter<>);
    }

    /// <summary>
    /// Applies this problem schema to the specified OpenAPI responses collection.
    /// </summary>
    /// <param name="responses">OpenAPI responses collection.</param>
    [Obsolete("Internal helper; it will become internal in Viana.Results.OpenApi 2.0.0.")]
    public void ApplyTo(OpenApiResponses? responses) => ApplyToCore(responses);

    internal void ApplyToCore(OpenApiResponses? responses)
    {
        if (responses == null) return;

        // Problem responses are documented with the RFC 9457 media type only (FR-016).
        responses[Status.ToString()] = BuildCore([ProblemContentType]);
    }

    /// <summary>
    /// Builds the final <see cref="OpenApiResponse"/> for the configured status code.
    /// </summary>
    /// <param name="jsonContentTypes">Content types that should receive the schema.</param>
    [Obsolete("Internal helper; it will become internal in Viana.Results.OpenApi 2.0.0.")]
    public OpenApiResponse Build(string[] jsonContentTypes) => BuildCore(jsonContentTypes);

    internal OpenApiResponse BuildCore(string[] jsonContentTypes)
    {
        var apiTypes = new Dictionary<string, OpenApiMediaType>();
        foreach (var type in jsonContentTypes)
            apiTypes[type] = new OpenApiMediaType
            {
                Schema = CreateSchema(),
                Example = BuildNode()
            };

        return new OpenApiResponse
        {
            Description = _title,
            Content = apiTypes
        };
    }

    private JsonNode? BuildNode()
    {
        // A real JSON object: numbers/booleans keep their type and text is escaped by the serializer.
        var node = new JsonObject();
        foreach (var field in _schemas)
            node[field.Name] = field.ExampleValue is null ? null : JsonSerializer.SerializeToNode(field.ExampleValue, field.ExampleValue.GetType(), _options);
        return node;
    }

    internal const string ProblemContentType = "application/problem+json";

    private static bool IsBaseMember(string name) => Array.Exists(ProblemSchemaRegistry.BaseMembers, m => string.Equals(m, name, StringComparison.OrdinalIgnoreCase));

    // Native-like base members (FR-013) plus this response's extra members; the document step replaces
    // it with the shared component (FR-017).
    private IOpenApiSchema CreateSchema()
    {
        var schema = ProblemSchemaRegistry.CreateBaseSchema();
        var properties = new Dictionary<string, IOpenApiSchema>(schema.Properties!, StringComparer.OrdinalIgnoreCase);
        var extras = new Dictionary<string, IOpenApiSchema>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in _schemas)
        {
            if (IsBaseMember(field.Name))
                continue;
            var member = field.ToOpenApi();
            properties[field.Name] = member;
            extras[field.Name] = member;
        }

        schema.Properties = properties;
        ProblemSchemaRegistry.Register(schema, extras);
        return schema;
    }

    private static string GetDefaultDescription(int statusCode)
    {
        // Same default as the runtime ProblemResult title (feature 002): reason phrase, or "Error".
        return HttpStatusInfo.TryGetReasonPhrase(statusCode, out var phrase) ? phrase : "Error";
    }
}
