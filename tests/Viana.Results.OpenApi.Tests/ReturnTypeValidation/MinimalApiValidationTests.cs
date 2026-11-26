using System.Reflection;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Feature 013 US1: Viana declarations on a minimal API that does not return a result fail.</summary>
public class MinimalApiValidationTests
{
    private static readonly MethodInfo Text = typeof(MinimalHandlers).GetMethod(nameof(MinimalHandlers.Text))!;
    private static readonly MethodInfo Typed = typeof(MinimalHandlers).GetMethod(nameof(MinimalHandlers.Typed))!;

    public static TheoryData<string, Action<StubEndpointBuilder>> Declarations() => new()
    {
        { "ProducesResult", b => b.ProducesResult<ReturnUser>() },
        { "ProducesResult", b => b.ProducesResult<OkUserExample>() },
        { "ProducesProblemResult", b => b.ProducesProblemResult(404, title: "Missing") },
        { "ProducesProblemResult", b => b.ProducesProblemResult<NotFoundProblemExample>() },
        { "WithoutDefaultResult", b => b.Add(e => e.Metadata.Add(new WithoutDefaultResultAttribute())) },
    };

    [Theory]
    [MemberData(nameof(Declarations))]
    public void Declaration_on_a_non_result_handler_fails(string declaration, Action<StubEndpointBuilder> declare)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Ops.RunMinimal(Text, StubEndpointBuilder.With(declare)));

        Assert.Contains("GET /x", ex.Message);
        Assert.Contains(declaration, ex.Message);
        Assert.Contains("String", ex.Message);
    }

    [Theory]
    [MemberData(nameof(Declarations))]
    public void Declaration_on_a_result_handler_applies(string declaration, Action<StubEndpointBuilder> declare)
    {
        Assert.NotEmpty(declaration);
        var operation = Ops.RunMinimal(Typed, StubEndpointBuilder.With(declare));

        Assert.NotNull(operation.Responses);
    }

    [Fact]
    public void Global_examples_on_a_non_result_handler_neither_fail_nor_apply()
    {
        var options = new VianaResultOptions();
        options.AddExample<GlobalErrorExample>(500);

        var operation = Ops.RunMinimal(Text, new List<object>(), options);

        Assert.False(operation.Responses!.ContainsKey("500"));
    }
}
