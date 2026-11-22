using System.Reflection;
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
        #pragma warning disable CS0618 // exercises the obsolete Description option until it is removed in 2.0.0
        Assert.Null(attr.Description);
        #pragma warning restore CS0618
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var attr = new ProblemResultAttribute(422)
        {
            Title = "Validation failed",
            Type = "https://example.com/validation",
            #pragma warning disable CS0618 // exercises the obsolete Description option until it is removed in 2.0.0
            Description = "One or more fields are invalid."
            #pragma warning restore CS0618
        };

        Assert.Equal(422, attr.Status);
        Assert.Equal("Validation failed", attr.Title);
        Assert.Equal("https://example.com/validation", attr.Type);
        #pragma warning disable CS0618 // exercises the obsolete Description option until it is removed in 2.0.0
        Assert.Equal("One or more fields are invalid.", attr.Description);
        #pragma warning restore CS0618
    }

    [Fact]
    public void Attribute_AllowsMultiple()
    {
        var usage = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(
            typeof(ProblemResultAttribute), typeof(AttributeUsageAttribute));

        Assert.NotNull(usage);
        Assert.True(usage.AllowMultiple);
        Assert.True(usage.Inherited);
    }

    [Fact]
    public void Detail_DefaultsToNull_AndCanBeSet()
    {
        Assert.Null(new ProblemResultAttribute(400).Detail);
        Assert.Equal("One or more fields are invalid.", new ProblemResultAttribute(400) { Detail = "One or more fields are invalid." }.Detail);
    }

    [Fact]
    public void Description_IsObsoleteWithRemovalNotice()
    {
        var property = typeof(ProblemResultAttribute).GetProperty("Description");
        var obsolete = property?.GetCustomAttribute<ObsoleteAttribute>();

        Assert.NotNull(obsolete);
        Assert.False(obsolete.IsError);
        Assert.Contains("no effect", obsolete.Message);
        Assert.Contains("2.0.0", obsolete.Message);
        Assert.Contains("Detail", obsolete.Message);
    }
}
