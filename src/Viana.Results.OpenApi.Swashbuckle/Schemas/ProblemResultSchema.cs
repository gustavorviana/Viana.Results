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

public class ProblemResultSchema
{
    private readonly List<ProblemFieldSchema> _schemas = [];
    private readonly JsonSerializerOptions _options;
    private readonly string _title;

    public int Status { get; }

    public ProblemResultSchema(JsonSerializerOptions options, ProblemResult result) : this(options, result.Status, result.Type, result.Title)
    {
        foreach (var extension in result.Extensions)
            AddField(GetType(extension.Value?.GetType()), extension.Key, "", extension.Value);
    }

    public ProblemResultSchema(JsonSerializerOptions options, int status, string type, string title)
    {
        _options = options;
        _title = title;

        Status = status;

        _schemas.AddRange([
            new ProblemFieldSchema(JsonSchemaType.Integer, "status", "RFC 9457: HTTP status code", status),
            new ProblemFieldSchema(JsonSchemaType.String, "type", "RFC 9457: problem type URI", type),
            new ProblemFieldSchema(JsonSchemaType.String, "title", "RFC 9457: short, human-readable summary", title)
        ]);
    }

    public static ProblemResultSchema FromAttribute(JsonSerializerOptions options, ProblemResultAttribute attribute)
    {
        return new ProblemResultSchema(options, attribute.Status, attribute.Type ?? "aboult:blank", attribute.Title ?? GetDefaultDescription(attribute.Status));
    }

    public ProblemResultSchema AddField(ProblemFieldSchema field)
    {
        _schemas.Add(field);
        return this;
    }

    public ProblemResultSchema AddField(JsonSchemaType type, string name, string description, object? exampleValue = null)
    {
        _schemas.Add(new ProblemFieldSchema(type, name, description, exampleValue));
        return this;
    }

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