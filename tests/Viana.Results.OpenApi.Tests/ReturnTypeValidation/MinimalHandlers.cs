namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Stand-ins for minimal API handlers (their MethodInfo is what the pipelines see).</summary>
public static class MinimalHandlers
{
    public static string Text() => "x";

    public static Task<Result<ReturnUser>> Typed() => Task.FromResult(new Result<ReturnUser>(new ReturnUser()));

    public static Result Bare() => Results.Ok();
}
