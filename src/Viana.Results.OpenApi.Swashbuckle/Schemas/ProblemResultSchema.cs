using Microsoft.OpenApi;
#if NET8_0
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Viana.Results.OpenApi.Swashbuckle.Schemas;

/// <summary>
/// Represents an OpenAPI schema builder for RFC 9457 Problem Details responses.
/// Produces standardized Swagger documentation including default and custom fields.
/// </summary>
public class ProblemResultSchema
{
    private readonly List<ProblemFieldSchema> _schemas = [];
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
            AddField(GetType(extension.Value?.GetType()), extension.Key, "", extension.Value);
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
            new ProblemFieldSchema(JsonSchemaType.Integer, "status", "RFC 9457: HTTP status code", status),
            new ProblemFieldSchema(JsonSchemaType.String, "type", "RFC 9457: problem type URI", type),
            new ProblemFieldSchema(JsonSchemaType.String, "title", "RFC 9457: short, human-readable summary", _title)
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
        return new ProblemResultSchema(options, attribute.Status, attribute.Type, attribute.Title);
    }

    /// <summary>
    /// Adds a custom field definition to the problem schema.
    /// </summary>
    /// <param name="field">Field schema definition.</param>
    /// <returns>The current instance for fluent chaining.</returns>
    public ProblemResultSchema AddField(ProblemFieldSchema field)
    {
        _schemas.Add(field);
        return this;
    }

    /// <summary>
    /// Adds a custom field definition using a JSON schema type.
    /// </summary>
    /// <param name="type">JSON schema type.</param>
    /// <param name="name">Field name.</param>
    /// <param name="description">Field description.</param>
    /// <param name="exampleValue">Optional example value.</param>
    /// <returns>The current instance for fluent chaining.</returns>
    public ProblemResultSchema AddField(JsonSchemaType type, string name, string description, object? exampleValue = null)
    {
        _schemas.Add(new ProblemFieldSchema(type, name, description, exampleValue));
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
    public ProblemResultSchema AddField(Type? type, string name, string description, object? exampleValue = null)
    {
        _schemas.Add(new ProblemFieldSchema(GetType(type), name, description, exampleValue));
        return this;
    }

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
    public void ApplyTo(OpenApiResponses? responses)
    {
        if (responses == null) return;

        var jsonTypes = responses
            .Select(x => x.Value)
            .SelectMany(x => x.Content ?? new Dictionary<string, OpenApiMediaType>())
            .Select(x => x.Key)
            .Where(x => x.Contains("json", StringComparison.OrdinalIgnoreCase) || string.Equals(x, "text/plain", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        responses[Status.ToString()] = Build(jsonTypes);
    }

    /// <summary>
    /// Builds the final <see cref="OpenApiResponse"/> for the configured status code.
    /// </summary>
    /// <param name="jsonContentTypes">Content types that should receive the schema.</param>
    public OpenApiResponse Build(string[] jsonContentTypes)
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

#if NET10_0_OR_GREATER
    private JsonNode? BuildNode()
    {
        return $"{{{string.Join(",", _schemas.Select(x => x.GetExampleJsonField()))}}}";
    }
#else
    private IOpenApiAny? BuildNode()
    {
        var json = $"{{{string.Join(",", _schemas.Select(x => x.GetExampleJsonField()))}}}";

        return OpenApiAnyFactory.CreateFromJson(json);
    }
#endif

#if NET10_0_OR_GREATER
    private IOpenApiSchema CreateSchema()
#else
    private OpenApiSchema CreateSchema()
#endif
    {
#if NET10_0_OR_GREATER
        var properties = new Dictionary<string, IOpenApiSchema>(StringComparer.OrdinalIgnoreCase);
#else
        var properties = new Dictionary<string, OpenApiSchema>(StringComparer.OrdinalIgnoreCase);
#endif

        foreach (var extension in _schemas)
            properties.TryAdd(extension.Name, extension.ToOpenApi());

        return new OpenApiSchema
        {
#if NET10_0_OR_GREATER
            Type = JsonSchemaType.Object,
#else
            Type = "object",
#endif
            Properties = properties
        };
    }

    private static string GetDefaultDescription(int statusCode)
    {
        if (ResultMessages.TryGet(statusCode, out var description) && !string.IsNullOrEmpty(description))
            return description;

        return "Internal Server Error";
    }
}