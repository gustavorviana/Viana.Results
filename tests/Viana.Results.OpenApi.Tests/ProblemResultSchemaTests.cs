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
        var result = schema.AddField(JsonSchemaType.String, "detail", "Details");
        Assert.Same(schema, result);
    }

    [Fact]
    public void AddField_WithClrType_ReturnsThis()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 400);
        var result = schema.AddField(typeof(string), "detail", "Details");
        Assert.Same(schema, result);
    }

    [Fact]
    public void Build_WithEmptyContentTypes_ReturnsEmptyContent()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 404);
        var response = schema.Build([]);
        Assert.NotNull(response.Content);
        Assert.Empty(response.Content);
        Assert.Equal("The Requested Resource Was Not Found.", response.Description);
    }

    [Fact]
    public void Build_WithJsonContentType_ReturnsContentWithSchema()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 404);
        var response = schema.Build(["application/json"]);
        Assert.NotNull(response.Content);
        Assert.Single(response.Content);
        Assert.True(response.Content.ContainsKey("application/json"));
    }

    private static IDictionary<string, IOpenApiSchema> PropertiesOf(OpenApiResponse response)
    {
        Assert.NotNull(response.Content);
        var schema = response.Content["application/json"].Schema;
        Assert.NotNull(schema);
        Assert.NotNull(schema.Properties);
        return schema.Properties;
    }

    [Fact]
    public void FromAttribute_WithDetail_AddsDetailProperty()
    {
        var attr = new ProblemResultAttribute(400) { Detail = "One or more fields are invalid." };

        var response = ProblemResultSchema.FromAttribute(DefaultOptions, attr).Build(["application/json"]);
        var properties = PropertiesOf(response);

        Assert.True(properties.ContainsKey("detail"));
        Assert.Equal(JsonSchemaType.String, properties["detail"].Type);
        Assert.Contains("One or more fields are invalid.", response.Content!["application/json"].Example!.ToJsonString());
    }

    [Fact]
    public void FromAttribute_WithoutDetail_KeepsStandardPropertiesOnly()
    {
        var response = ProblemResultSchema.FromAttribute(DefaultOptions, new ProblemResultAttribute(400)).Build(["application/json"]);

        Assert.Equal(new[] { "status", "title", "type" }, PropertiesOf(response).Keys.OrderBy(k => k).ToArray());
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

        OpenApiOperationProcessor.ApplyProblemResultAttributes(operation, method, DefaultOptions);

        Assert.True(operation.Responses.ContainsKey("400"));
        Assert.True(PropertiesOf((OpenApiResponse)operation.Responses["400"]).ContainsKey("detail"));
    }
}
