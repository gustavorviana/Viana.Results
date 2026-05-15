using Microsoft.OpenApi;
using Viana.Results.OpenApi.Schemas;

namespace Viana.Results.OpenApi.Tests;

public class ProblemFieldSchemaTests
{
    [Fact]
    public void ToOpenApi_CreatesSchemaWithDescription()
    {
        var field = new ProblemFieldSchema(JsonSchemaType.String, "title", "A title");
        var schema = field.ToOpenApi();
        Assert.Equal("A title", schema.Description);
    }

    [Fact]
    public void GetExampleJsonField_FormatsCorrectly()
    {
        var field = new ProblemFieldSchema(JsonSchemaType.String, "title", "A title", "Not Found");
        var json = field.GetExampleJsonField();
        Assert.Equal("\"title\":\"Not Found\"", json);
    }

    [Fact]
    public void GetExampleJsonValue_NullValue_ReturnsNull()
    {
        var field = new ProblemFieldSchema(JsonSchemaType.String, "title", "A title");
        Assert.Equal("null", field.GetExampleJsonValue());
    }

    [Fact]
    public void GetExampleJsonValue_IntValue_ReturnsString()
    {
        var field = new ProblemFieldSchema(JsonSchemaType.Integer, "status", "Status", 404);
        Assert.Equal("404", field.GetExampleJsonValue());
    }
}
