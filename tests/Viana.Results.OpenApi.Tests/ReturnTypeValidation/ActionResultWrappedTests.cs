namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Feature 013 (T004): ActionResult&lt;T&gt; wrapping a Viana result is a result return.</summary>
public class ActionResultWrappedTests
{
    [Fact]
    public void ActionResult_of_bare_Result_documents_204()
    {
        var operation = Ops.RunController<ActionResultWrappedController>(nameof(ActionResultWrappedController.Bare));

        Assert.False(operation.Responses!.ContainsKey("200"));
        Assert.True(operation.Responses.ContainsKey("204"));
    }

    [Fact]
    public void ActionResult_of_typed_Result_documents_its_problem()
    {
        var operation = Ops.RunController<ActionResultWrappedController>(nameof(ActionResultWrappedController.Typed));

        Assert.True(operation.Responses!.ContainsKey("404"));
    }
}
