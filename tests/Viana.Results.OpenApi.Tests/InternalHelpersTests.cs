namespace Viana.Results.OpenApi.Tests;

/// <summary>Feature 007 M1 (revised): the helpers are internal (breaking change of Viana.Results.OpenApi 2.0.0).</summary>
public class InternalHelpersTests
{
    [Theory]
    [InlineData("Viana.Results.OpenApi.Processing.OpenApiOperationProcessor")]
    [InlineData("Viana.Results.OpenApi.Processing.OpenApiDocumentProcessor")]
    [InlineData("Viana.Results.OpenApi.Processing.OpenApiSchemaProcessor")]
    [InlineData("Viana.Results.OpenApi.ResponseExampleResolver")]
    [InlineData("Viana.Results.OpenApi.ResolvedExample")]
    [InlineData("Viana.Results.OpenApi.Schemas.ProblemFieldSchema")]
    [InlineData("Viana.Results.OpenApi.Schemas.ProblemResultSchema")]
    public void Helper_is_not_public(string typeName)
    {
        var type = typeof(ProblemResultAttribute).Assembly.GetType(typeName, throwOnError: true)!;

        Assert.False(type.IsPublic);
    }
}
