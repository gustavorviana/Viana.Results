using System.Reflection;
using System.Text.Json;
using Microsoft.OpenApi;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Tests;

/// <summary>Feature 008 US3: example providers returning real Viana results (controllers).</summary>
public class ResultExampleTests
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public sealed class User { public string Name { get; set; } = "Ana"; }

    public sealed class OkUser : IExampleProvider<Result<User>> { public Result<User> GetExample() => Viana.Results.Results.Ok(new User()); }

    public sealed class UserList : IExampleProvider<ListResult<User>> { public ListResult<User> GetExample() => new(new List<User> { new(), new() }); }

    public sealed class UserPage : IExampleProvider<PagedResult<User>> { public PagedResult<User> GetExample() => new(new List<User> { new() }, 1, 4); }

    public sealed class Failed : IExampleProvider<Result> { public Result GetExample() => new ProblemBuilder(409).WithTitle("Conflict!").BuildResult(); }

    public sealed class RawUser : IExampleProvider<User> { public User GetExample() => new() { Name = "Raw" }; }

    public sealed class Endpoints
    {
        [ResponseExample(typeof(OkUser))] public Result<User> Ok() => Viana.Results.Results.Ok(new User());
        [ResponseExample(typeof(UserList))] public ListResult<User> List() => new(new List<User>());
        [ResponseExample(typeof(UserPage))] public PagedResult<User> Page() => new(new List<User>(), 1, 1);
        [ResponseExample(typeof(Failed))] public Result Fail() => Viana.Results.Results.Ok();
        [ResponseExample(201, typeof(OkUser))] public Result<User> Mismatch() => Viana.Results.Results.Ok(new User());
        [ResponseExample(200, typeof(RawUser))] public Result<User> Raw() => Viana.Results.Results.Ok(new User());
    }

    private static OpenApiOperation Run(string method)
    {
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new() { Schema = new OpenApiSchema() } } }
            }
        };
        var info = typeof(Endpoints).GetMethod(method, BindingFlags.Public | BindingFlags.Instance)!;
        OperationProcessor.ApplyResponseExamples(operation, info, Json, null, null);
        return operation;
    }

    private static System.Text.Json.Nodes.JsonNode Example(OpenApiOperation op, string status) => op.Responses![status].Content!["application/json"].Example!;

    [Fact]
    public void TypedResult_ExampleIsUnwrappedValue_StatusFromResult()
    {
        Assert.Equal("Ana", Example(Run(nameof(Endpoints.Ok)), "200")["name"]!.GetValue<string>());
    }

    [Fact]
    public void ListResult_ExampleIsArray()
    {
        Assert.Equal(2, Example(Run(nameof(Endpoints.List)), "200").AsArray().Count);
    }

    [Fact]
    public void PagedResult_ExampleKeepsMetadata()
    {
        Assert.Equal(4, Example(Run(nameof(Endpoints.Page)), "200")["totalPages"]!.GetValue<int>());
    }

    [Fact]
    public void FailedResult_IsProblemExampleForItsStatus()
    {
        var op = Run(nameof(Endpoints.Fail));

        Assert.Equal("Conflict!", op.Responses!["409"].Content!["application/problem+json"].Example!["title"]!.GetValue<string>());
    }

    [Fact]
    public void ExplicitStatus_DifferentFromResult_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Run(nameof(Endpoints.Mismatch)));

        Assert.Contains("201", ex.Message);
        Assert.Contains("200", ex.Message);
    }

    [Fact]
    public void RawValueProvider_IsUnchanged()
    {
        Assert.Equal("Raw", Example(Run(nameof(Endpoints.Raw)), "200")["name"]!.GetValue<string>());
    }

    [Fact]
    public void AttributeWithoutStatus_ExposesZero()
    {
        Assert.Equal(0, new ResponseExampleAttribute(typeof(OkUser)).StatusCode);
    }
}
