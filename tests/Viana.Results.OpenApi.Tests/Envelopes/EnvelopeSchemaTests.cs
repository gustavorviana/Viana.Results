using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Viana.Results.OpenApi.Tests.ReturnTypeValidation;

namespace Viana.Results.OpenApi.Tests.Envelopes;

/// <summary>Feature 014 US4: the documented envelope is the shape the runtime writes, on both pipelines.</summary>
public class EnvelopeSchemaTests
{
    private static async Task<JsonElement> Document(string generator)
    {
        var document = await ValidationHost.Generate(generator, [typeof(DocCursorController)], app =>
            app.MapGet("/v014/minimal", () => new DocCursorExample().GetExample()).ProducesResult<DocCursorExample>());
        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private static JsonElement Resolve(JsonElement root, JsonElement schema)
    {
        while (schema.TryGetProperty("$ref", out var reference))
            schema = root.GetProperty("components").GetProperty("schemas").GetProperty(reference.GetString()!.Split('/').Last());
        if (schema.TryGetProperty("allOf", out var all) && all.GetArrayLength() == 1)
            return Resolve(root, all[0]);
        return schema;
    }

    private static JsonElement Success(JsonElement root, string path)
    {
        var media = root.GetProperty("paths").GetProperty(path).GetProperty("get").GetProperty("responses").GetProperty("200")
            .GetProperty("content").GetProperty("application/json");
        return media;
    }

    private static string[] Members(JsonElement root, string path)
        => Resolve(root, Success(root, path).GetProperty("schema")).GetProperty("properties").EnumerateObject()
            .Select(p => p.Name).OrderBy(n => n).ToArray();

    [Theory]
    [MemberData(nameof(ValidationHost.Generators), MemberType = typeof(ValidationHost))]
    public async Task Envelope_schema_has_the_runtime_members(string generator)
    {
        var root = await Document(generator);

        Assert.Equal(["items", "nextCursor"], Members(root, "/v014/cursor"));
        Assert.Equal(["items", "nextCursor"], Members(root, "/v014/minimal"));
    }

    [Theory]
    [MemberData(nameof(ValidationHost.Generators), MemberType = typeof(ValidationHost))]
    public async Task Envelope_with_extensions_allows_additional_properties(string generator)
    {
        var root = await Document(generator);

        var schema = Resolve(root, Success(root, "/v014/extended").GetProperty("schema"));
        Assert.Equal(["data"], schema.GetProperty("properties").EnumerateObject().Select(p => p.Name).ToArray());
        Assert.True(schema.TryGetProperty("additionalProperties", out var additional) && additional.ValueKind != JsonValueKind.False);
    }

    [Theory]
    [MemberData(nameof(ValidationHost.Generators), MemberType = typeof(ValidationHost))]
    public async Task Example_is_the_runtime_body(string generator)
    {
        var root = await Document(generator);
        var expected = ((System.Text.Json.Nodes.JsonNode)ResultResponseBody.GetBody(
            new DocCursorExample().GetExample(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!).ToJsonString();

        foreach (var path in new[] { "/v014/cursor", "/v014/minimal" })
        {
            var media = Success(root, path);
            var example = media.TryGetProperty("example", out var single) ? single : media.GetProperty("examples").EnumerateObject().First().Value.GetProperty("value");
            Assert.Equal(expected, System.Text.Json.Nodes.JsonNode.Parse(example.GetRawText())!.ToJsonString());
        }
    }
}
