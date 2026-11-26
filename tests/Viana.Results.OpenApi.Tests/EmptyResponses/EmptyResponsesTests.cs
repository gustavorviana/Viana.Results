using System.Text.Json;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Tests.EmptyResponses;

/// <summary>Feature 015 US2: generated responses are kept on operations without responses.</summary>
public class EmptyResponsesTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Authorized_endpoint_with_null_responses_documents_401_and_403()
    {
        var operation = new OpenApiOperation { Responses = null };
        var method = typeof(SecuredController).GetMethod(nameof(SecuredController.Get))!;

        OperationProcessor.ApplyAuthorizeResponses(operation, null, method, Json);

        Assert.NotNull(operation.Responses);
        Assert.True(operation.Responses.ContainsKey("401"));
        Assert.True(operation.Responses.ContainsKey("403"));
    }

    [Fact]
    public void Problem_example_on_empty_responses_documents_the_problem_but_not_the_success()
    {
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var method = typeof(ProblemOnlyController).GetMethod(nameof(ProblemOnlyController.Get))!;

        OperationProcessor.ApplyResponseExamples(operation, method, Json, null, null);

        Assert.True(operation.Responses.ContainsKey("404"));
        Assert.False(operation.Responses.ContainsKey("200"));
    }
}
