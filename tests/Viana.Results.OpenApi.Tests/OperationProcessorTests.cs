using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Tests;

public class OperationProcessorTests
{
    private static readonly JsonSerializerOptions Json = new();

    private static OpenApiOperation NewOperation() => new()
    {
        Responses = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse { Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new() } }
        }
    };

    private static MethodInfo Method<T>(string name) =>
        typeof(T).GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)!;

    private sealed class Actions
    {
        public ValueTask<Result> BareValueTask() => new(Results.Ok());

        public Task<Result> BareTask() => Task.FromResult(Results.Ok());

        [ProblemResult(404)]
        public ValueTask<Result<int>> TypedValueTask() => new(Results.Ok(1));
    }

    [Authorize]
    private sealed class SecuredController
    {
        public Result Secured() => Results.Ok();

        [AllowAnonymous]
        public Result Anonymous() => Results.Ok();
    }

    [Theory]
    [InlineData(nameof(Actions.BareValueTask))]
    [InlineData(nameof(Actions.BareTask))]
    public void ClearBodyForBareResult_HandlesTaskAndValueTask(string name)
    {
        var operation = NewOperation();

        OperationProcessor.ClearBodyForBareResult(operation, Method<Actions>(name));

        Assert.Empty(operation.Responses!["200"].Content!);
    }

    [Fact]
    public void ApplyProblemResultAttributes_HandlesValueTask()
    {
        var operation = NewOperation();

        OperationProcessor.ApplyProblemResultAttributes(operation, Method<Actions>(nameof(Actions.TypedValueTask)), Json);

        Assert.True(operation.Responses!.ContainsKey("404"));
    }

    [Fact]
    public void ApplyAuthorizeResponses_AllowAnonymousAction_AddsNothing()
    {
        var operation = NewOperation();

        OperationProcessor.ApplyAuthorizeResponses(operation, null, Method<SecuredController>(nameof(SecuredController.Anonymous)), Json);

        Assert.False(operation.Responses!.ContainsKey("401"));
        Assert.False(operation.Responses.ContainsKey("403"));
    }

    [Fact]
    public void ApplyAuthorizeResponses_AuthorizedController_Adds401And403()
    {
        var operation = NewOperation();

        OperationProcessor.ApplyAuthorizeResponses(operation, null, Method<SecuredController>(nameof(SecuredController.Secured)), Json);

        Assert.True(operation.Responses!.ContainsKey("401"));
        Assert.True(operation.Responses.ContainsKey("403"));
    }

    [Fact]
    public void ApplyAuthorizeResponses_EndpointMetadataRequiringAuthorization_Adds401And403()
    {
        var operation = NewOperation();

        OperationProcessor.ApplyAuthorizeResponses(operation, new object[] { new AuthorizeAttribute() }, null, Json);

        Assert.True(operation.Responses!.ContainsKey("401"));
        Assert.True(operation.Responses.ContainsKey("403"));
    }

    [Fact]
    public void ApplyAuthorizeResponses_EndpointMetadataWithAllowAnonymous_AddsNothing()
    {
        var operation = NewOperation();

        OperationProcessor.ApplyAuthorizeResponses(operation, new object[] { new AuthorizeAttribute(), new AllowAnonymousAttribute() }, null, Json);

        Assert.False(operation.Responses!.ContainsKey("401"));
    }
}
