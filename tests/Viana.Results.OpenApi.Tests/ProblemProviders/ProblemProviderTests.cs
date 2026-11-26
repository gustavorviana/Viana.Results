using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Tests.ProblemProviders;

/// <summary>Feature 012 US1: ProblemResult example providers on minimal APIs, documented like on controllers.</summary>
public class ProblemProviderTests
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        OperationProcessor.ApplyEndpointDeclarations(operation, builder.Metadata, Json, null, _ => new OpenApiSchema(), "GET /x");
        return operation;
    }

    private static System.Text.Json.Nodes.JsonNode Example(OpenApiOperation operation, string status)
    {
        var media = operation.Responses![status].Content!["application/problem+json"];
        return media.Example ?? media.Examples!.Values.First().Value!;
    }

    private static void AssertUnavailable(OpenApiOperation operation)
    {
        Assert.True(operation.Responses!.ContainsKey("503"));
        var example = Example(operation, "503");
        Assert.Equal("Unavailable", example["title"]!.GetValue<string>());
        Assert.Equal("Try again later.", example["detail"]!.GetValue<string>());
        Assert.Equal(5, example["retryAfterSeconds"]!.GetValue<int>());
    }

    [Fact]
    public void ProducesProblemResult_with_a_ProblemResult_provider_documents_its_problem()
        => AssertUnavailable(Minimal(b => b.ProducesProblemResult<UnavailableProblem>()));

    [Fact]
    public void ProducesResult_with_a_ProblemResult_provider_is_the_same_problem_and_no_200()
    {
        var operation = Minimal(b => b.ProducesResult<UnavailableProblem>());

        AssertUnavailable(operation);
        Assert.False(operation.Responses!.ContainsKey("200"));
    }

    [Fact]
    public void Inline_problem_wrapped_in_a_Result_documents_the_same_problem()
        => AssertUnavailable(Minimal(b => b.ProducesProblemResult(new Result(new UnavailableProblem().GetExample()))));

    [Fact]
    public void Problem_provider_returning_something_else_fails_with_a_clear_message()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Minimal(b => b.ProducesProblemResult<NotAResult>()));

        Assert.Contains("GET /x", ex.Message);
        Assert.Contains(nameof(NotAResult), ex.Message);
    }

    [Fact]
    public void Explicit_status_that_differs_from_the_problem_fails()
        => Assert.Throws<InvalidOperationException>(() => Minimal(b => b.ProducesResult<UnavailableProblem>(status: 500)));

    [Fact]
    public void Controller_and_minimal_document_the_provider_identically()
    {
        // As in the real pipeline, the controller operation already has its inferred response.
        var controller = new OpenApiOperation { Responses = new OpenApiResponses { ["204"] = new OpenApiResponse() } };
        var method = typeof(ProblemProviderController).GetMethod(nameof(ProblemProviderController.Get))!;
        OperationProcessor.ApplyResponseExamples(controller, method, Json, null, null);

        var minimal = Minimal(b => b.ProducesProblemResult<UnavailableProblem>());

        Assert.Equal(Example(controller, "503").ToJsonString(), Example(minimal, "503").ToJsonString());
    }
}
