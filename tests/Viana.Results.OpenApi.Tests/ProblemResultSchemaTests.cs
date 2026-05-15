using System.Text.Json;
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
        Assert.Empty(response.Content);
        Assert.Equal("The Requested Resource Was Not Found.", response.Description);
    }

    [Fact]
    public void Build_WithJsonContentType_ReturnsContentWithSchema()
    {
        var schema = new ProblemResultSchema(DefaultOptions, 404);
        var response = schema.Build(["application/json"]);
        Assert.Single(response.Content);
        Assert.True(response.Content.ContainsKey("application/json"));
    }
}
