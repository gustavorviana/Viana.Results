using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Viana.Results;

/// <summary>
/// JSON converter for <see cref="ProblemResult"/> that serializes/deserializes according to RFC 9457:
/// standard members "type", "title", "status" at root level, with extension members (e.g. "description", "errors") at the same level.
/// </summary>
public sealed class ProblemResultJsonConverter : JsonConverter<ProblemResult>
{
	private const string TypeName = "type";
	private const string TitleName = "title";
	private const string StatusName = "status";

	/// <inheritdoc />
	public override ProblemResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected start of object for ProblemResult.");

		string? type = null;
		string? title = null;
		int? status = null;
		string? description = null;
		var extensions = new Dictionary<string, object?>(StringComparer.Ordinal);

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				break;

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected property name.");

			var propName = reader.GetString()!;
			reader.Read();

			if (propName.Equals(TypeName, StringComparison.OrdinalIgnoreCase))
				type = reader.GetString();
			else if (propName.Equals(TitleName, StringComparison.OrdinalIgnoreCase))
				title = reader.GetString();
			else if (propName.Equals(StatusName, StringComparison.OrdinalIgnoreCase))
				status = reader.TokenType == JsonTokenType.Number ? reader.GetInt32() : null;
			else
				extensions[propName] = ReadExtensionValue(ref reader, options);
		}

		if (string.IsNullOrWhiteSpace(title))
			title = "Error";
		if (string.IsNullOrWhiteSpace(type))
			type = "about:blank";

		// "description" might have been read as an extension; use it for WithDescription
		if (extensions.TryGetValue("description", out var descObj))
		{
			extensions.Remove("description");
			description = descObj as string ?? descObj?.ToString();
		}

		return string.IsNullOrEmpty(description)
			? new ProblemResult(status ?? 500, title, type, extensions.Count > 0 ? extensions : null)
			: ProblemResult.WithDescription(status ?? 500, title, description, type, extensions.Count > 0 ? extensions : null);
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, ProblemResult value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString(TypeName, value.Type);
		writer.WriteString(TitleName, value.Title);
		writer.WriteNumber(StatusName, value.Status);

		if (value.Extensions != null)
		{
			foreach (var kv in value.Extensions)
			{
				writer.WritePropertyName(kv.Key);
				WriteExtensionValue(writer, kv.Value, options);
			}
		}

		writer.WriteEndObject();
	}

	private static object? ReadExtensionValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.String:
				return reader.GetString();
			case JsonTokenType.Number:
				if (reader.TryGetInt32(out var i))
					return i;
				if (reader.TryGetInt64(out var l))
					return l;
				return reader.GetDouble();
			case JsonTokenType.True:
			case JsonTokenType.False:
				return reader.GetBoolean();
			case JsonTokenType.Null:
				return null;
			case JsonTokenType.StartObject:
			case JsonTokenType.StartArray:
				return JsonSerializer.Deserialize<JsonElement>(ref reader, options);
			default:
				throw new JsonException($"Unexpected token type for extension value: {reader.TokenType}");
		}
	}

	private static void WriteExtensionValue(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		switch (value)
		{
			case string s:
				writer.WriteStringValue(s);
				break;
			case int i:
				writer.WriteNumberValue(i);
				break;
			case long l:
				writer.WriteNumberValue(l);
				break;
			case double d:
				writer.WriteNumberValue(d);
				break;
			case bool b:
				writer.WriteBooleanValue(b);
				break;
			default:
				JsonSerializer.Serialize(writer, value, value.GetType(), options);
				break;
		}
	}
}
