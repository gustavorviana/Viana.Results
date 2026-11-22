using System.Text.Json;
using Viana.Results.OpenApi.Processing;
using Microsoft.OpenApi;
using Viana.Results.OpenApi.Schemas;

namespace Viana.Results.OpenApi.Tests;

public class ProblemResultSchemaTests
{
    private static readonly JsonSerializerOptions DefaultOptions = new();

    [Fact]
    public void Constructor_WithStatus_AddsDefaultFields()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 404);
        Assert.Equal(404, schema.Status);
    }

    [Fact]
    public void Constructor_WithProblemResult_SetsStatus()
    {
        var problem = new ProblemResult(404, "Not Found");
        var schema = new ProblemResultSchema(DefaultOptions, problem);
        Assert.Equal(404, schema.Status);
    }

    [Fact]
    public void FromAttribute_CreatesSchema()
    {
        var attr = new ProblemResultAttribute(400) { Title = "Bad Request" };
        var schema = ProblemResultSchema.FromAttribute(DefaultOptions, attr);
        Assert.Equal(400, schema.Status);
    }

    [Fact]
    public void AddField_WithJsonSchemaType_ReturnsThis()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 400);
        var result = schema.AddFieldCore(JsonSchemaType.String, "detail", "Details");
        Assert.Same(schema, result);
    }

    [Fact]
    public void AddField_WithClrType_ReturnsThis()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 400);
        var result = schema.AddFieldCore(typeof(string), "detail", "Details");
        Assert.Same(schema, result);
    }

    [Fact]
    public void Build_WithEmptyContentTypes_ReturnsEmptyContent()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 404);
        var response = schema.BuildCore([]);
        Assert.NotNull(response.Content);
        Assert.Empty(response.Content);
        Assert.Equal("Not Found", response.Description);
    }

    [Fact]
    public void Build_WithJsonContentType_ReturnsContentWithSchema()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 404);
        var response = schema.BuildCore(["application/json"]);
        Assert.NotNull(response.Content);
        Assert.Single(response.Content);
        Assert.True(response.Content.ContainsKey("application/json"));
    }

    private static IDictionary<string, IOpenApiSchema> PropertiesOf(OpenApiResponse response)
    {
        Assert.NotNull(response.Content);
        var schema = response.Content.Values.First().Schema;
        Assert.NotNull(schema);
        Assert.NotNull(schema.Properties);
        return schema.Properties;
    }

    [Fact]
    public void FromAttribute_WithDetail_AddsDetailProperty()
    {
        var attr = new ProblemResultAttribute(400) { Detail = "One or more fields are invalid." };

        var response = ProblemResultSchema.FromAttribute(DefaultOptions, attr).BuildCore(["application/json"]);
        var properties = PropertiesOf(response);

        Assert.True(properties.ContainsKey("detail"));
        Assert.True(properties["detail"].Type!.Value.HasFlag(JsonSchemaType.String));
        Assert.Contains("One or more fields are invalid.", response.Content!["application/json"].Example!.ToJsonString());
    }

    [Fact]
    public void FromAttribute_WithoutDetail_HasNativeBaseMembersOnly()
    {
        var response = ProblemResultSchema.FromAttribute(DefaultOptions, new ProblemResultAttribute(400)).BuildCore(["application/json"]);

        Assert.Equal(new[] { "detail", "instance", "status", "title", "type" }, PropertiesOf(response).Keys.OrderBy(k => k).ToArray());
    }

    [ProblemResult(400, Detail = "Invalid input.")]
    private static Result AnnotatedAction() => Results.Ok();

    [Fact]
    public void ApplyProblemResultAttributes_UsesDetail_ForBothPipelines()
    {
        // Native transformers and Swashbuckle's ProblemResponseOperationFilter both call this processor.
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new() } }
            }
        };
        var method = typeof(ProblemResultSchemaTests).GetMethod(nameof(AnnotatedAction), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

        OperationProcessor.ApplyProblemResultAttributes(operation, method, DefaultOptions);

        Assert.True(operation.Responses.ContainsKey("400"));
        Assert.True(PropertiesOf((OpenApiResponse)operation.Responses["400"]).ContainsKey("detail"));
    }

    private static System.Text.Json.Nodes.JsonNode ExampleOf(ProblemResultSchema schema)
    {
        var example = schema.BuildCore(["application/json"]).Content!["application/json"].Example;
        Assert.NotNull(example);
        return example;
    }

    [Fact]
    public void Example_IsJsonObjectWithTypedValues()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 404)
            .AddFieldCore(typeof(int), "count", "n", 3)
            .AddFieldCore(typeof(bool), "retry", "r", true)
            .AddFieldCore(typeof(string), "msg", "m", "say \"hi\"");

        var example = ExampleOf(schema);

        Assert.Equal(JsonValueKind.Object, example.GetValueKind());
        Assert.Equal(JsonValueKind.Number, example["status"]!.GetValueKind());
        Assert.Equal(JsonValueKind.Number, example["count"]!.GetValueKind());
        Assert.Equal(JsonValueKind.True, example["retry"]!.GetValueKind());
        using var doc = JsonDocument.Parse(example.ToJsonString());
        Assert.Equal("say \"hi\"", doc.RootElement.GetProperty("msg").GetString());
    }

    [Theory]
    [InlineData(404, "Not Found")]
    [InlineData(422, "Unprocessable Content")]
    [InlineData(499, "Error")]
    public void DefaultTitle_MatchesRuntimeReasonPhrase(int status, string expected)
    {
        var schema = new ProblemResultSchema(DefaultOptions, status);

        Assert.Equal(expected, ExampleOf(schema)["title"]!.GetValue<string>());
        Assert.Equal(expected, schema.BuildCore(["application/json"]).Description);
    }

    [Fact]
    public void Fields_NamesDifferingByCase_AreOneFieldWithFirstCasingAndLastExample()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 400)
            .AddFieldCore(typeof(string), "errorCode", "e", "E1")
            .AddFieldCore(typeof(string), "ErrorCode", "e", "E2");

        var response = schema.BuildCore(["application/json"]);
        var properties = response.Content!["application/json"].Schema!.Properties!;
        var example = response.Content["application/json"].Example!.AsObject();

        Assert.Single(properties, p => string.Equals(p.Key, "errorCode", StringComparison.OrdinalIgnoreCase));
        Assert.True(example.ContainsKey("errorCode"));
        Assert.False(example.ContainsKey("ErrorCode"));
        Assert.Equal("E2", example["errorCode"]!.GetValue<string>());
    }

    [Fact]
    public void Fields_FollowNamingPolicy()
    {
        var camel = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var example = new ProblemResultSchema(camel, 400).AddFieldCore(typeof(string), "TraceId", "t", "x")
            .BuildCore(["application/json"]).Content!["application/json"].Example!.AsObject();

        Assert.True(example.ContainsKey("traceId")); // FR-018 (006) supersedes the "no naming policy" rule of 007
    }
}
