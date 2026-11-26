using System.Reflection;
using System.Text.Json;
using Microsoft.OpenApi;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Tests;

/// <summary>FR-014 / FR-015: one source per status (endpoint example &gt; action attribute &gt; controller attribute &gt; global example).</summary>
public class ProblemSourcePrecedenceTests
{
    private static readonly JsonSerializerOptions Json = new();

    public sealed class ValidationExample : IExampleProvider<ProblemResult>
    {
        public ProblemResult GetExample() => new ProblemBuilder(400).WithTitle("Validation").AddExtension("errors", new[] { "x" }).Build();
    }

    public sealed class BusinessExample : IExampleProvider<ProblemResult>
    {
        public ProblemResult GetExample() => new ProblemBuilder(400).WithTitle("Business").AddExtension("rule", "R1").Build();
    }

    public sealed class GlobalServerError : IExampleProvider<ProblemResult>
    {
        public ProblemResult GetExample() => new ProblemBuilder(500).WithTitle("Global").AddExtension("incident", "I1").Build();
    }

    public sealed class Forecast { public int TemperatureC { get; set; } }

    public sealed class ForecastExample : IExampleProvider<Forecast>
    {
        public Forecast GetExample() => new() { TemperatureC = 22 };
    }

    [ProblemResult(500, Title = "Controller")]
    public sealed class Endpoints
    {
        [ProblemResult(500, Title = "Action")]
        public Result ActionBeatsController() => Results.Ok();

        public Result ControllerOnly() => Results.Ok();

        [ProblemResult(400, Title = "Attribute")]
        [ResponseExample(400, typeof(ValidationExample))]
        public Result ExampleBeatsAttribute() => Results.Ok();

        [ResponseExample(400, typeof(ValidationExample), Name = "validation")]
        [ResponseExample(400, typeof(BusinessExample), Name = "business")]
        public Result TwoExamples() => Results.Ok();

        [ResponseExample(200, typeof(ForecastExample))]
        public Result<Forecast> Success() => Results.Ok(new Forecast());
    }

    private static OpenApiOperation Run(string method, VianaResultOptions? options = null)
    {
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new() { Schema = new OpenApiSchema() } } }
            }
        };
        var info = typeof(Endpoints).GetMethod(method, BindingFlags.Public | BindingFlags.Instance)!;
        OperationProcessor.ApplyProblemResultAttributes(operation, info, Json);
        OperationProcessor.ApplyResponseExamples(operation, info, Json, null, options);
        return operation;
    }

    private static OpenApiMediaType Problem(OpenApiOperation operation, string status) => operation.Responses![status].Content!["application/problem+json"];

    private static string? ExampleTitle(OpenApiMediaType media) => media.Example?["title"]?.GetValue<string>();

    [Fact]
    public void ActionAttribute_BeatsControllerAttribute()
    {
        Assert.Equal("Action", ExampleTitle(Problem(Run(nameof(Endpoints.ActionBeatsController)), "500")));
    }

    [Fact]
    public void ControllerAttribute_AppliesWhenActionHasNone()
    {
        Assert.Equal("Controller", ExampleTitle(Problem(Run(nameof(Endpoints.ControllerOnly)), "500")));
    }

    [Fact]
    public void EndpointExample_BeatsAttribute_ForSchemaAndExample()
    {
        var media = Problem(Run(nameof(Endpoints.ExampleBeatsAttribute)), "400");

        Assert.Equal("Validation", ExampleTitle(media));
        Assert.True(media.Schema!.Properties!.ContainsKey("errors"));
    }

    [Fact]
    public void GlobalExample_IsIgnored_WhenEndpointDeclaresStatus()
    {
        var options = new VianaResultOptions().AddExample<GlobalServerError>(500);

        var media = Problem(Run(nameof(Endpoints.ControllerOnly), options), "500");

        Assert.Equal("Controller", ExampleTitle(media));
        Assert.False(media.Schema!.Properties!.ContainsKey("incident"));
    }

    [Fact]
    public void TwoEndpointExamples_SchemaIsUnionOfMembers()
    {
        var media = Problem(Run(nameof(Endpoints.TwoExamples)), "400");

        Assert.True(media.Schema!.Properties!.ContainsKey("errors"));
        Assert.True(media.Schema.Properties.ContainsKey("rule"));
        Assert.Equal(2, media.Examples!.Count);
    }

    [Fact]
    public void SuccessExamples_AreUnchanged()
    {
        var operation = Run(nameof(Endpoints.Success));
        var media = operation.Responses!["200"].Content!["application/json"];

        Assert.Equal(22, media.Example!["TemperatureC"]!.GetValue<int>());
        Assert.False(operation.Responses["200"].Content!.ContainsKey("application/problem+json"));
    }
}
