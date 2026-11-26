using System.Text.Json;

namespace Viana.Results.AspNetCore.Tests;

public class ProblemDetailsMapperTests
{
    private static Result Failure(int resultStatus, ProblemResult problem) => new(resultStatus, problem);

    private static ProblemResult Problem(int status, string title, string type = "about:blank", params (string Key, object? Value)[] ext)
        => new(status, title, type, ext.ToDictionary(e => e.Key, e => e.Value, StringComparer.Ordinal));

    [Fact]
    public void Status_ComesFromResult_NotFromProblem()
    {
        var details = ProblemDetailsMapper.ToProblemDetails(Failure(409, Problem(400, "x")), null, out _);

        Assert.Equal(409, details.Status);
    }

    [Fact]
    public void Title_IsCopied()
    {
        Assert.Equal("Custom", ProblemDetailsMapper.ToProblemDetails(Failure(400, Problem(400, "Custom")), null, out _).Title);
    }

    [Fact]
    public void AboutBlankType_BecomesNull_SoNativeLinkApplies()
    {
        Assert.Null(ProblemDetailsMapper.ToProblemDetails(Failure(404, Problem(404, "x")), null, out _).Type);
    }

    [Fact]
    public void ExplicitType_IsCopied()
    {
        var details = ProblemDetailsMapper.ToProblemDetails(Failure(404, Problem(404, "x", "https://e.com/p")), null, out _);

        Assert.Equal("https://e.com/p", details.Type);
    }

    [Fact]
    public void DetailAndInstance_ArePromoted()
    {
        var problem = Problem(400, "x", "about:blank", ("detail", "d"), ("instance", "/i"), ("errors", 1));

        var details = ProblemDetailsMapper.ToProblemDetails(Failure(400, problem), null, out _);

        Assert.Equal("d", details.Detail);
        Assert.Equal("/i", details.Instance);
        Assert.False(details.Extensions.ContainsKey("detail"));
        Assert.False(details.Extensions.ContainsKey("instance"));
        Assert.Equal(1, details.Extensions["errors"]);
    }

    [Fact]
    public void Description_StaysAnExtension()
    {
        var details = ProblemDetailsMapper.ToProblemDetails(Failure(400, Problem(400, "x", "about:blank", ("description", "legacy"))), null, out _);

        Assert.Null(details.Detail);
        Assert.Equal("legacy", details.Extensions["description"]);
    }

    [Fact]
    public void ExtensionNames_FollowNamingPolicy_AndAreCaseInsensitive()
    {
        var problem = Problem(400, "x", "about:blank", ("ErrorCode", "E"));

        var details = ProblemDetailsMapper.ToProblemDetails(Failure(400, problem), JsonNamingPolicy.CamelCase, out _);

        Assert.True(details.Extensions.ContainsKey("errorCode"));
        Assert.True(details.Extensions.ContainsKey("ERRORCODE"));
        Assert.DoesNotContain(details.Extensions.Keys, k => k == "ErrorCode");
    }

    [Fact]
    public void ExtensionNamesCollidingAfterPolicy_LastValueWins()
    {
        var problem = Problem(400, "x", "about:blank", ("ErrorCode", "first"), ("error_code", "second"));

        var details = ProblemDetailsMapper.ToProblemDetails(Failure(400, problem), JsonNamingPolicy.SnakeCaseLower, out _);

        Assert.Equal("second", Assert.Single(details.Extensions, e => e.Key == "error_code").Value);
    }

    [Fact]
    public void TraceId_IsRemovedAndReturnedForFallback()
    {
        var problem = Problem(400, "x", "about:blank", ("TraceId", "viana-trace"));

        var details = ProblemDetailsMapper.ToProblemDetails(Failure(400, problem), JsonNamingPolicy.CamelCase, out var vianaTraceId);

        Assert.False(details.Extensions.ContainsKey("traceId"));
        Assert.Equal("viana-trace", vianaTraceId);
    }
}
