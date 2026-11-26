namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Feature 013 US2: a Viana attribute placed on a non-result action fails.</summary>
public class ActionAttributeValidationTests
{
    [Theory]
    [InlineData(nameof(NonResultActionController.ProblemOnDto), "ProblemResult", "ReturnUser")]
    [InlineData(nameof(NonResultActionController.ProblemExampleOnActionResult), "ResponseExample", "IActionResult")]
    [InlineData(nameof(NonResultActionController.SuccessExampleOnTask), "ResponseExample", "Task")]
    [InlineData(nameof(NonResultActionController.WithoutDefaultOnDto), "WithoutDefaultResult", "ReturnUser")]
    public void Attribute_on_a_non_result_action_fails(string action, string attribute, string type)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Ops.RunController<NonResultActionController>(action));

        Assert.Contains($"{nameof(NonResultActionController)}.{action}", ex.Message);
        Assert.Contains($"[{attribute}]", ex.Message);
        Assert.Contains(type, ex.Message);
    }

    [Fact]
    public void Attribute_on_a_result_action_applies()
    {
        var operation = Ops.RunController<NonResultActionController>(nameof(NonResultActionController.ProblemOnResult));

        Assert.True(operation.Responses!.ContainsKey("404"));
    }
}
