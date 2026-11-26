using Microsoft.AspNetCore.Mvc;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Feature 013 R1: Viana results, possibly wrapped in Task/ValueTask/ActionResult, are result returns.</summary>
public class ResultReturnTests
{
    public static TheoryData<Type, Type> Accepted() => new()
    {
        { typeof(Result), typeof(Result) },
        { typeof(Result<ReturnUser>), typeof(Result<ReturnUser>) },
        { typeof(ListResult<ReturnUser>), typeof(ListResult<ReturnUser>) },
        { typeof(IResult), typeof(IResult) },
        { typeof(IResult<ReturnUser>), typeof(IResult<ReturnUser>) },
        { typeof(IListResult<ReturnUser>), typeof(IListResult<ReturnUser>) },
        { typeof(IPagedResult<ReturnUser>), typeof(IPagedResult<ReturnUser>) },
        { typeof(Task<Result<ReturnUser>>), typeof(Result<ReturnUser>) },
        { typeof(ValueTask<Result>), typeof(Result) },
        { typeof(Task<IResult>), typeof(IResult) },
        { typeof(ActionResult<Result<ReturnUser>>), typeof(Result<ReturnUser>) },
        { typeof(Task<ActionResult<Result<ReturnUser>>>), typeof(Result<ReturnUser>) },
        { typeof(Task<ActionResult<IResult<ReturnUser>>>), typeof(IResult<ReturnUser>) },
    };

    public static TheoryData<Type> Refused() => new()
    {
        typeof(IActionResult), typeof(ActionResult), typeof(object), typeof(Microsoft.AspNetCore.Http.IResult),
        typeof(void), typeof(Task), typeof(Task<ReturnUser>), typeof(ReturnUser), typeof(List<Result>),
        typeof(IEnumerable<Result<ReturnUser>>), typeof(Task<List<Result>>), typeof(Dictionary<string, Result>),
        typeof(ActionResult<ReturnUser>),
    };

    [Theory]
    [MemberData(nameof(Accepted))]
    public void Accepted_types_are_result_returns(Type type, Type expected)
    {
        Assert.True(ReturnTypeRules.IsResultReturn(type));
        Assert.Equal(expected, ReturnTypeRules.ResultTypeOf(type));
    }

    [Theory]
    [MemberData(nameof(Refused))]
    public void Refused_types_are_not_result_returns(Type type)
    {
        Assert.False(ReturnTypeRules.IsResultReturn(type));
        Assert.Null(ReturnTypeRules.ResultTypeOf(type));
    }
}
