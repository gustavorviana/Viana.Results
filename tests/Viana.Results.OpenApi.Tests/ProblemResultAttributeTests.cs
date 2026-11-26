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
#pragma warning disable CS0618 // exercises the obsolete Description option
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
#pragma warning disable CS0618 // exercises the obsolete Description option
            Description = "One or more fields are invalid."
#pragma warning restore CS0618
        };

        Assert.Equal(422, attr.Status);
        Assert.Equal("Validation failed", attr.Title);
        Assert.Equal("https://example.com/validation", attr.Type);
#pragma warning disable CS0618 // exercises the obsolete Description option
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
    public void Description_IsObsoleteAndNamesTheReplacement()
    {
        var property = typeof(ProblemResultAttribute).GetProperty("Description");
        var obsolete = property?.GetCustomAttribute<ObsoleteAttribute>();

        Assert.NotNull(obsolete);
        Assert.False(obsolete.IsError);
        Assert.Contains("no effect", obsolete.Message);
        Assert.DoesNotMatch(@"\d+\.\d+\.\d+", obsolete.Message); // feature 015: no future version
        Assert.Contains("Detail", obsolete.Message);
    }
}
