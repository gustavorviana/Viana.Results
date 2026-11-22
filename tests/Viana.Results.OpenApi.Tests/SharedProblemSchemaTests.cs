using System.Text.Json;
using Microsoft.OpenApi;
using Viana.Results.OpenApi.Processing;
using Viana.Results.OpenApi.Schemas;

namespace Viana.Results.OpenApi.Tests;

/// <summary>FR-013, FR-016, FR-017: native-like base schema, problem content type, shared component.</summary>
public class SharedProblemSchemaTests
{
    private static readonly JsonSerializerOptions Json = new();

    private static (OpenApiDocument Document, OpenApiResponses Responses) DocumentWith(params ProblemResultSchema[] schemas)
    {
        var responses = new OpenApiResponses();
        foreach (var schema in schemas)
            schema.ApplyToCore(responses);

        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/x"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation> { [HttpMethod.Get] = new OpenApiOperation { Responses = responses } }
                }
            }
        };
        return (document, responses);
    }

    [Fact]
    public void ApplyToCore_UsesOnlyProblemContentType()
    {
        var responses = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse { Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new(), ["text/plain"] = new() } }
        };

        new ProblemResultSchema(Json, 404).ApplyToCore(responses);

        Assert.Equal(new[] { "application/problem+json" }, responses["404"].Content!.Keys.ToArray());
    }

    [Fact]
    public void BaseMembers_MatchNativeProblemDetails_WithoutTraceId()
    {
        var (document, _) = DocumentWith(new ProblemResultSchema(Json, 404));

        DocumentProcessor.ApplySharedProblemSchema(document);

        var component = document.Components!.Schemas![DocumentProcessor.ProblemComponentName];
        Assert.Equal(new[] { "detail", "instance", "status", "title", "type" }, component.Properties!.Keys.OrderBy(k => k).ToArray());
        Assert.True(component.Required == null || component.Required.Count == 0);
        Assert.All(component.Properties.Values, p => Assert.True(p.Type!.Value.HasFlag(JsonSchemaType.Null)));
    }

    [Fact]
    public void ResponseWithoutExtras_ReferencesTheSharedComponent()
    {
        var (document, responses) = DocumentWith(new ProblemResultSchema(Json, 404));

        DocumentProcessor.ApplySharedProblemSchema(document);

        var schema = responses["404"].Content!["application/problem+json"].Schema;
        Assert.IsType<OpenApiSchemaReference>(schema);
    }

    [Fact]
    public void ResponseWithExtras_UsesAllOfBasePlusOwnMembers()
    {
        var (document, responses) = DocumentWith(new ProblemResultSchema(Json, 400).AddFieldCore(typeof(string), "errorCode", "e", "E1"));

        DocumentProcessor.ApplySharedProblemSchema(document);

        var schema = responses["400"].Content!["application/problem+json"].Schema!;
        Assert.Equal(2, schema.AllOf!.Count);
        Assert.IsType<OpenApiSchemaReference>(schema.AllOf[0]);
        Assert.Equal(new[] { "errorCode" }, schema.AllOf[1].Properties!.Keys.ToArray());
    }

    [Fact]
    public void ExtensionNames_FollowNamingPolicy()
    {
        var camel = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var media = new ProblemResultSchema(camel, 400).AddFieldCore(typeof(string), "ErrorCode", "e", "E1")
            .BuildCore(["application/problem+json"]).Content!["application/problem+json"];

        Assert.True(media.Schema!.Properties!.ContainsKey("errorCode"));
        Assert.True(media.Example!.AsObject().ContainsKey("errorCode"));
    }
}
