using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.OpenApi;
using Viana.Results.OpenApi.Processing;
using Viana.Results.OpenApi.Schemas;

namespace Viana.Results.OpenApi.Tests;

/// <summary>Feature 008 FR-011: a problem without its own type documents what the runtime sends.</summary>
public class ProblemTypeDefaultsTests
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Same links the ASP.NET Core runtime writes (asserted on the runtime side in the AspNetCore tests).</summary>
    public static readonly TheoryData<int, string> Native = new()
    {
        { 404, "https://tools.ietf.org/html/rfc9110#section-15.5.5" },
        { 409, "https://tools.ietf.org/html/rfc9110#section-15.5.10" },
        { 500, "https://tools.ietf.org/html/rfc9110#section-15.6.1" },
    };

    private const string Custom = "https://example.com/problems/custom";

    public sealed class NotFound : IExampleProvider<Result> { public Result GetExample() => new ProblemBuilder(404).WithTitle("x").BuildResult(); }
    public sealed class Teapot : IExampleProvider<Result> { public Result GetExample() => new ProblemBuilder(418).WithTitle("x").BuildResult(); }
    public sealed class Typed : IExampleProvider<Result> { public Result GetExample() => new ProblemBuilder(404).WithType(Custom).WithTitle("x").BuildResult(); }
    public sealed class ServerError : IExampleProvider<Result> { public Result GetExample() => new ProblemBuilder(500).WithTitle("x").BuildResult(); }

    public sealed class Endpoints
    {
        [ResponseExample(typeof(NotFound))] public Result Missing() => Viana.Results.Results.Ok();
        [ResponseExample(typeof(Typed))] public Result Explicit() => Viana.Results.Results.Ok();
        public Result Plain() => Viana.Results.Results.Ok();
    }

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

    private static OpenApiOperation Minimal(Action<StubBuilder> declare)
    {
        var builder = new StubBuilder();
        declare(builder);
        var op = new OpenApiOperation { Responses = new OpenApiResponses() };
        OperationProcessor.ApplyEndpointDeclarations(op, builder.Metadata, Json, null, _ => new OpenApiSchema(), "GET /x");
        return op;
    }

    private static OpenApiOperation Controller(string method, VianaResultOptions? options = null)
    {
        var op = new OpenApiOperation { Responses = new OpenApiResponses { ["200"] = new OpenApiResponse() } };
        var info = typeof(Endpoints).GetMethod(method, BindingFlags.Public | BindingFlags.Instance)!;
        OperationProcessor.ApplyResponseExamples(op, info, Json, null, options);
        return op;
    }

    private static string? TypeOf(OpenApiOperation op, int status)
    {
        var media = op.Responses![status.ToString()].Content!["application/problem+json"];
        var node = media.Example ?? media.Examples!.Values.First().Value;
        return node!["type"]!.GetValue<string>();
    }

    [Theory, MemberData(nameof(Native))]
    public void Schema_WithoutType_UsesNativeLink(int status, string link)
    {
        var response = new ProblemResultSchema(Json, status).BuildCore([ProblemResultSchema.ProblemContentType]);

        Assert.Equal(link, response.Content!.Values.First().Example!["type"]!.GetValue<string>());
    }

    [Theory, MemberData(nameof(Native))]
    public void MinimalMetadata_WithoutType_UsesNativeLink(int status, string link)
        => Assert.Equal(link, TypeOf(Minimal(b => b.ProducesProblemResult(status, title: "x")), status));

    [Theory, MemberData(nameof(Native))]
    public void MinimalInlineResult_WithoutType_UsesNativeLink(int status, string link)
        => Assert.Equal(link, TypeOf(Minimal(b => b.ProducesProblemResult(new ProblemBuilder(status).WithTitle("x").BuildResult())), status));

    [Fact]
    public void MinimalExampleClass_WithoutType_UsesNativeLink()
        => Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", TypeOf(Minimal(b => b.ProducesProblemResult<NotFound>()), 404));

    [Fact]
    public void ControllerExample_WithoutType_UsesNativeLink()
        => Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", TypeOf(Controller(nameof(Endpoints.Missing)), 404));

    [Fact]
    public void GlobalExample_WithoutType_UsesNativeLink()
        => Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1",
            TypeOf(Controller(nameof(Endpoints.Plain), new VianaResultOptions().AddExample<ServerError>(500)), 500));

    [Fact]
    public void UnmappedStatus_KeepsAboutBlank()
    {
        Assert.Equal("about:blank", TypeOf(Minimal(b => b.ProducesProblemResult<Teapot>()), 418));
        Assert.Equal("about:blank", TypeOf(Minimal(b => b.ProducesProblemResult(418)), 418));
    }

    [Fact]
    public void ExplicitType_IsNeverChanged()
    {
        Assert.Equal(Custom, TypeOf(Controller(nameof(Endpoints.Explicit)), 404));
        Assert.Equal(Custom, TypeOf(Minimal(b => b.ProducesProblemResult(404, type: Custom)), 404));
    }
}
