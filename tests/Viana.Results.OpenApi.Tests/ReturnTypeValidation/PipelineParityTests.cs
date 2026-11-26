using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Feature 013 FR-007: both pipelines apply the same rules (US1, US2 and US3 end to end).</summary>
public class PipelineParityTests
{
    [Theory]
    [MemberData(nameof(ValidationHost.Generators), MemberType = typeof(ValidationHost))]
    public async Task Minimal_api_without_result_fails(string generator)
    {
        var ex = await ValidationHost.Failure(generator, [], app =>
            app.MapGet("/v013/text", () => "x").ProducesResult<ReturnUser>());

        Assert.Contains("GET /v013/text", ex.Message);
        Assert.Contains("ProducesResult", ex.Message);
    }

    [Theory]
    [MemberData(nameof(ValidationHost.Generators), MemberType = typeof(ValidationHost))]
    public async Task Minimal_api_with_result_is_documented(string generator)
    {
        var document = await ValidationHost.Generate(generator, [], app =>
            app.MapGet("/v013/user", () => new Result<ReturnUser>(new ReturnUser())).ProducesProblemResult(404, title: "Missing"));

        Assert.Contains("404", document.Statuses("/v013/user"));
    }

    [Theory]
    [MemberData(nameof(ValidationHost.Generators), MemberType = typeof(ValidationHost))]
    public async Task Action_attribute_without_result_fails(string generator)
    {
        var ex = await ValidationHost.Failure(generator, [typeof(PipelineActionController)]);

        Assert.Contains("PipelineActionController.Get", ex.Message);
        Assert.Contains("[ProblemResult]", ex.Message);
    }

    [Theory]
    [MemberData(nameof(ValidationHost.Generators), MemberType = typeof(ValidationHost))]
    public async Task Class_attribute_applies_to_result_actions_and_warns_for_the_others(string generator)
    {
        var logs = new ListLoggerProvider();

        var document = await ValidationHost.Generate(generator, [typeof(PipelineMixedController)], logs: logs);

        Assert.Contains("500", document.Statuses("/v013/mixed/result"));
        Assert.DoesNotContain("500", document.Statuses("/v013/mixed/dto"));
        var warning = Assert.Single(logs.Logger.Entries);
        Assert.Equal(LogLevel.Warning, warning.Level);
        Assert.Contains("PipelineMixedController.Dto", warning.Message);
    }

    [Theory]
    [MemberData(nameof(ValidationHost.Generators), MemberType = typeof(ValidationHost))]
    public async Task Class_attribute_without_any_result_action_fails(string generator)
    {
        var ex = await ValidationHost.Failure(generator, [typeof(PipelineNoResultController)]);

        Assert.Contains("Controller 'PipelineNoResultController'", ex.Message);
    }
}
