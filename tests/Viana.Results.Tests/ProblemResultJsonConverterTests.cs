using System.Text.Json;

namespace Viana.Results.Tests;

public class ProblemResultJsonConverterTests
{
	private static readonly JsonSerializerOptions Options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	[Fact]
	public void Serialize_MinimalProblem_WritesTypeTitleStatus()
	{
		var problem = new ProblemResult(400, "Bad Request");

		var json = JsonSerializer.Serialize(problem, Options);

		Assert.Contains("\"type\":\"about:blank\"", json);
		Assert.Contains("\"title\":\"Bad Request\"", json);
		Assert.Contains("\"status\":400", json);
	}

	[Fact]
	public void Serialize_WithExtensions_WritesExtensionsAtRootLevel()
	{
		var extensions = new Dictionary<string, object?>(StringComparer.Ordinal)
		{
			["description"] = "Invalid input",
			["traceId"] = "abc-123"
		};
		var problem = ProblemResult.WithDescription(404, "Not Found", "Resource missing", "about:blank", extensions);

		var json = JsonSerializer.Serialize(problem, Options);

		Assert.Contains("\"type\":\"about:blank\"", json);
		Assert.Contains("\"title\":\"Not Found\"", json);
		Assert.Contains("\"status\":404", json);
		Assert.Contains("\"description\":\"Resource missing\"", json);
		Assert.Contains("\"traceId\":\"abc-123\"", json);
	}

	[Fact]
	public void Deserialize_MinimalJson_ReturnsProblemResult()
	{
		var json = """{"type":"about:blank","title":"Bad Request","status":400}""";

		var problem = JsonSerializer.Deserialize<ProblemResult>(json, Options);

		Assert.NotNull(problem);
		Assert.Equal(400, problem.Status);
		Assert.Equal("Bad Request", problem.Title);
		Assert.Equal("about:blank", problem.Type);
		Assert.NotNull(problem.Extensions);
		Assert.Empty(problem.Extensions);
	}

	[Fact]
	public void Deserialize_WithDescription_StoresInExtensions()
	{
		var json = """{"type":"about:blank","title":"Validation failed","status":400,"description":"One or more fields are invalid."}""";

		var problem = JsonSerializer.Deserialize<ProblemResult>(json, Options);

		Assert.NotNull(problem);
		Assert.Equal(400, problem.Status);
		Assert.True(problem.Extensions.TryGetValue("description", out var desc));
		Assert.Equal("One or more fields are invalid.", desc);
	}

	[Fact]
	public void Deserialize_WithExtensionMembers_PopulatesExtensions()
	{
		var json = """{"type":"about:blank","title":"Bad Request","status":400,"traceId":"req-123","count":1}""";

		var problem = JsonSerializer.Deserialize<ProblemResult>(json, Options);

		Assert.NotNull(problem);
		Assert.True(problem.Extensions.TryGetValue("traceId", out var traceId));
		Assert.Equal("req-123", traceId);
		Assert.True(problem.Extensions.TryGetValue("count", out var count));
		Assert.Equal(1, Convert.ToInt32(count));
	}

	[Fact]
	public void Deserialize_PropertyNamesCaseInsensitive_ReadsCorrectly()
	{
		var json = """{"TYPE":"about:blank","Title":"Error","STATUS":500}""";

		var problem = JsonSerializer.Deserialize<ProblemResult>(json, Options);

		Assert.NotNull(problem);
		Assert.Equal(500, problem.Status);
		Assert.Equal("Error", problem.Title);
		Assert.Equal("about:blank", problem.Type);
	}

	[Fact]
	public void Deserialize_EmptyTitle_DefaultsToError()
	{
		var json = """{"type":"about:blank","title":"","status":404}""";

		var problem = JsonSerializer.Deserialize<ProblemResult>(json, Options);

		Assert.NotNull(problem);
		Assert.Equal("Error", problem.Title);
	}

	[Fact]
	public void RoundTrip_ProblemResult_PreservesData()
	{
		var extensions = new Dictionary<string, object?>(StringComparer.Ordinal)
		{
			["description"] = "Detail text",
			["key"] = "value"
		};
		var original = ProblemResult.WithDescription(422, "Unprocessable Entity", "Detail text", "https://api.example.com/err", extensions);

		var json = JsonSerializer.Serialize(original, Options);
		var roundTripped = JsonSerializer.Deserialize<ProblemResult>(json, Options);

		Assert.NotNull(roundTripped);
		Assert.Equal(original.Status, roundTripped.Status);
		Assert.Equal(original.Title, roundTripped.Title);
		Assert.Equal(original.Type, roundTripped.Type);
		Assert.True(roundTripped.Extensions.TryGetValue("description", out var d));
		Assert.Equal("Detail text", d);
		Assert.True(roundTripped.Extensions.TryGetValue("key", out var k));
		Assert.Equal("value", k);
	}

	[Fact]
	public void Deserialize_WithErrorsObject_StoresAsJsonElementInExtensions()
	{
		var json = """{"type":"about:blank","title":"Validation failed","status":400,"errors":{"Email":["Invalid"],"Name":["Required"]}}""";

		var problem = JsonSerializer.Deserialize<ProblemResult>(json, Options);

		Assert.NotNull(problem);
		Assert.True(problem.Extensions.TryGetValue("errors", out var errors));
		Assert.NotNull(errors);
		Assert.IsType<JsonElement>(errors);
		var element = (JsonElement)errors;
		Assert.Equal(JsonValueKind.Object, element.ValueKind);
		Assert.True(element.TryGetProperty("Email", out var emailArr));
		Assert.Equal(1, emailArr.GetArrayLength());
		Assert.Equal("Invalid", emailArr[0].GetString());
	}

	[Fact]
	public void Serialize_WithErrorsExtension_RoundTrips()
	{
		var extensions = new Dictionary<string, object?>(StringComparer.Ordinal)
		{
			["errors"] = new Dictionary<string, string[]> { ["Email"] = new[] { "Invalid" } }
		};
		var problem = new ProblemResult(400, "Validation failed", "about:blank", extensions);

		var json = JsonSerializer.Serialize(problem, Options);
		var deserialized = JsonSerializer.Deserialize<ProblemResult>(json, Options);

		Assert.NotNull(deserialized);
		Assert.Equal(400, deserialized.Status);
		Assert.True(deserialized.Extensions.TryGetValue("errors", out var err));
		Assert.NotNull(err);
	}

	[Fact]
	public void Deserialize_NotStartObject_ThrowsJsonException()
	{
		var json = """[{"type":"about:blank","title":"x","status":400}]""";

		var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ProblemResult>(json, Options));

		Assert.Contains("Expected start of object", ex.Message);
	}
}
