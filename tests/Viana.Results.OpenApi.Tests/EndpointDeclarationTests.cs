using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.OpenApi;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Tests;

/// <summary>Feature 008 US2: ProducesResult / ProducesProblemResult on minimal API endpoints.</summary>
public class EndpointDeclarationTests
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public sealed class User { public string Name { get; set; } = "Ana"; }

    public sealed class UserOkExample : IExampleProvider<Result<User>>
    {
        public Result<User> GetExample() => Viana.Results.Results.Ok(new User { Name = "Ana" });
    }

    public sealed class NotFoundExample : IExampleProvider<Result>
    {
        public Result GetExample() => new ProblemBuilder(404).WithTitle("User not found").AddExtension("userId", 7).BuildResult();
    }

    public sealed class ConflictExample : IExampleProvider<Result>
    {
        public Result GetExample() => new ProblemBuilder(404).WithTitle("Other").AddExtension("reason", "x").BuildResult();
    }

    public sealed class NullUserExample : IExampleProvider<Result<User>>
    {
        public Result<User> GetExample() => new((User?)null);
    }

    /// <summary>Collects the metadata added by endpoint conventions.</summary>
    private sealed class StubBuilder : IEndpointConventionBuilder
    {
        public List<object> Metadata { get; } = new();

        public void Add(Action<EndpointBuilder> convention)
        {
            var endpoint = new RouteEndpointBuilder(_ => Task.CompletedTask, RoutePatternFactory.Parse("/x"), 0);
            convention(endpoint);
            Metadata.AddRange(endpoint.Metadata);
        }
    }

    private static async Task<OpenApiOperation> RunAsync(Action<StubBuilder> declare, VianaResultOptions? globals = null)
    {
        var builder = new StubBuilder();
        declare(builder);
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        if (globals != null)
            operation.Responses["500"] = new OpenApiResponse { Content = new Dictionary<string, OpenApiMediaType> { ["application/problem+json"] = new() } };
        await OperationProcessor.ApplyEndpointDeclarationsAsync(operation, builder.Metadata, Json, null,
            t => ValueTask.FromResult<IOpenApiSchema>(new OpenApiSchema { Description = "schema:" + t.Name }), "GET /x");
        return operation;
    }

    private static OpenApiMediaType Media(OpenApiOperation op, string status, string contentType) => op.Responses![status].Content![contentType];

    [Fact]
    public async Task ProducesResult_DataType_Documents200WithTypeSchema()
    {
        var op = await RunAsync(b => b.ProducesResult<User>());

        Assert.Equal("schema:User", Media(op, "200", "application/json").Schema!.Description);
    }

    [Fact]
    public async Task ProducesResult_ExampleClass_StatusAndUnwrappedExampleFromResult()
    {
        var op = await RunAsync(b => b.ProducesResult<UserOkExample>());

        var media = Media(op, "200", "application/json");
        Assert.Equal("schema:User", media.Schema!.Description);
        Assert.Equal("Ana", media.Example!["name"]!.GetValue<string>());
    }

    [Fact]
    public async Task ProducesResult_InlineValue_SameAsExampleClass()
    {
        var op = await RunAsync(b => b.ProducesResult(Viana.Results.Results.Ok(new User { Name = "Bia" })));

        Assert.Equal("Bia", Media(op, "200", "application/json").Example!["name"]!.GetValue<string>());
    }

    [Fact]
    public async Task ProducesResult_NullValueExample_Documents204WithoutBody()
    {
        var op = await RunAsync(b => b.ProducesResult<NullUserExample>());

        Assert.True(op.Responses!.ContainsKey("204"));
        Assert.True(op.Responses["204"].Content == null || op.Responses["204"].Content!.Count == 0);
    }

    [Fact]
    public async Task ProducesProblemResult_Metadata_DocumentsProblem()
    {
        var op = await RunAsync(b => b.ProducesProblemResult(404, title: "Missing", detail: "Nope"));

        var media = Media(op, "404", "application/problem+json");
        Assert.Equal("Missing", media.Example!["title"]!.GetValue<string>());
        Assert.Equal("Nope", media.Example["detail"]!.GetValue<string>());
    }

    [Fact]
    public async Task ProducesProblemResult_ExampleClass_Generic_Type_AndInline_StatusFromResult()
    {
        var generic = await RunAsync(b => b.ProducesProblemResult<NotFoundExample>());
        var byType = await RunAsync(b => b.ProducesProblemResult(typeof(NotFoundExample)));
        var inline = await RunAsync(b => b.ProducesProblemResult(new NotFoundExample().GetExample()));

        foreach (var op in new[] { generic, byType, inline })
        {
            var media = Media(op, "404", "application/problem+json");
            Assert.Equal("User not found", media.Example!["title"]!.GetValue<string>());
            Assert.True(media.Schema!.Properties!.ContainsKey("userId"));
        }
    }

    [Fact]
    public async Task ExplicitStatus_DifferentFromResult_Throws()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(b => b.ProducesResult<UserOkExample>(status: 201)));

        Assert.Contains("201", ex.Message);
        Assert.Contains("200", ex.Message);
        Assert.Contains("GET /x", ex.Message);
    }

    [Fact]
    public async Task TwoProblemExamples_SameStatus_UnionSchemaAndBothExamples()
    {
        var op = await RunAsync(b => b.ProducesProblemResult<NotFoundExample>(name: "a").ProducesProblemResult<ConflictExample>(name: "b"));

        var media = Media(op, "404", "application/problem+json");
        Assert.True(media.Schema!.Properties!.ContainsKey("userId"));
        Assert.True(media.Schema.Properties.ContainsKey("reason"));
        Assert.Equal(2, media.Examples!.Count);
    }

    [Fact]
    public async Task EndpointExample_BeatsEndpointMetadata_ForSameStatus()
    {
        var op = await RunAsync(b => b.ProducesProblemResult(404, title: "Metadata").ProducesProblemResult<NotFoundExample>());

        Assert.Equal("User not found", Media(op, "404", "application/problem+json").Example!["title"]!.GetValue<string>());
    }

    [Fact]
    public async Task EndpointDeclaration_ReplacesGlobalExampleResponse()
    {
        var op = await RunAsync(b => b.ProducesProblemResult(500, title: "Endpoint"), new VianaResultOptions());

        Assert.Equal("Endpoint", Media(op, "500", "application/problem+json").Example!["title"]!.GetValue<string>());
    }
}
