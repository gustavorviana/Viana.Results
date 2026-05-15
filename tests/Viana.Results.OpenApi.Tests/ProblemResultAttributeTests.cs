using Viana.Results.OpenApi;

namespace Viana.Results.OpenApi.Tests;

public class ProblemResultAttributeTests
{
    [Fact]
    public void Constructor_SetsStatus()
    {
        var attr = new ProblemResultAttribute(404);
        Assert.Equal(404, attr.Status);
    }

    [Fact]
    public void Properties_DefaultToNull()
    {
        var attr = new ProblemResultAttribute(400);
        Assert.Null(attr.Title);
        Assert.Null(attr.Type);
        Assert.Null(attr.Description);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var attr = new ProblemResultAttribute(422)
        {
            Title = "Validation failed",
            Type = "https://example.com/validation",
            Description = "One or more fields are invalid."
        };

        Assert.Equal(422, attr.Status);
        Assert.Equal("Validation failed", attr.Title);
        Assert.Equal("https://example.com/validation", attr.Type);
        Assert.Equal("One or more fields are invalid.", attr.Description);
    }

    [Fact]
    public void Attribute_AllowsMultiple()
    {
        var usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
            typeof(ProblemResultAttribute), typeof(AttributeUsageAttribute));

        Assert.NotNull(usage);
        Assert.True(usage.AllowMultiple);
        Assert.True(usage.Inherited);
    }
}
