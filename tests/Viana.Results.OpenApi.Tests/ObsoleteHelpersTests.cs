using System.Reflection;

namespace Viana.Results.OpenApi.Tests;

public class ObsoleteHelpersTests
{
    private static readonly Assembly OpenApi = typeof(ProblemResultAttribute).Assembly;

    [Theory]
    [InlineData("Viana.Results.OpenApi.Processing.OpenApiOperationProcessor")]
    [InlineData("Viana.Results.OpenApi.Processing.OpenApiDocumentProcessor")]
    [InlineData("Viana.Results.OpenApi.Processing.OpenApiSchemaProcessor")]
    [InlineData("Viana.Results.OpenApi.ResponseExampleResolver")]
    [InlineData("Viana.Results.OpenApi.ResolvedExample")]
    [InlineData("Viana.Results.OpenApi.Schemas.ProblemFieldSchema")]
    public void Helpers_AreObsoleteWithInternalNotice(string typeName)
    {
        var type = OpenApi.GetType(typeName, throwOnError: true)!;

        AssertNotice(type.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Theory]
    [InlineData("AddField")]
    [InlineData("Build")]
    [InlineData("ApplyTo")]
    public void ProblemResultSchemaLowLevelMembers_AreObsolete(string member)
    {
        var methods = OpenApi.GetType("Viana.Results.OpenApi.Schemas.ProblemResultSchema", throwOnError: true)!
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == member)
            .ToList();

        Assert.NotEmpty(methods);
        foreach (var m in methods)
            AssertNotice(m.GetCustomAttribute<ObsoleteAttribute>());
    }

    private static void AssertNotice(ObsoleteAttribute? obsolete)
    {
        Assert.NotNull(obsolete);
        Assert.False(obsolete.IsError);
        Assert.Contains("internal", obsolete.Message);
        Assert.Contains("2.0.0", obsolete.Message);
    }
}
