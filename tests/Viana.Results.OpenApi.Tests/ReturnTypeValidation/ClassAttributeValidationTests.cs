using Microsoft.Extensions.Logging;
using Viana.Results.OpenApi.Processing;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Feature 013 US3: class-level attributes apply to result actions only.</summary>
public class ClassAttributeValidationTests
{
    [Fact]
    public void Result_actions_receive_the_class_declarations()
    {
        var logger = new ListLogger();

        Assert.True(Ops.RunController<MixedController>(nameof(MixedController.Typed), logger).Responses!.ContainsKey("500"));
        Assert.True(Ops.RunController<MixedController>(nameof(MixedController.Interface), logger).Responses!.ContainsKey("500"));
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public void Non_result_actions_are_skipped_with_one_warning_per_attribute()
    {
        var logger = new ListLogger();

        var operation = Ops.RunController<MixedController>(nameof(MixedController.Dto), logger);

        Assert.False(operation.Responses!.ContainsKey("500"));
        Assert.False(operation.Responses.ContainsKey("404"));
        Assert.Equal(2, logger.Entries.Count);
        Assert.All(logger.Entries, e => Assert.Equal(LogLevel.Warning, e.Level));
        Assert.Contains(logger.Entries, e => e.Message.Contains("MixedController.Dto") && e.Message.Contains("[ProblemResult]"));
        Assert.Contains(logger.Entries, e => e.Message.Contains("MixedController.Dto") && e.Message.Contains("[ResponseExample]"));
    }

    [Fact]
    public void Two_generations_log_the_warning_twice()
    {
        var logger = new ListLogger();

        Ops.RunController<MixedController>(nameof(MixedController.Dto), logger);
        Ops.RunController<MixedController>(nameof(MixedController.Dto), logger);

        Assert.Equal(4, logger.Entries.Count);
    }

    [Fact]
    public void Controller_without_result_action_fails()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Ops.RunController<NoResultController>(nameof(NoResultController.Dto)));

        Assert.Contains(nameof(NoResultController), ex.Message);
        Assert.Contains("[ProblemResult]", ex.Message);
    }

    [Fact]
    public void Controller_whose_only_result_action_returns_the_interface_has_a_result_action()
    {
        var operation = Ops.RunController<OnlyInterfaceController>(nameof(OnlyInterfaceController.Dto), new ListLogger());

        Assert.False(operation.Responses!.ContainsKey("500"));
    }

    [Fact]
    public void Inherited_class_attributes_and_actions_follow_the_derived_controller()
    {
        var logger = new ListLogger();

        var inherited = Ops.RunController<DerivedController>(nameof(DerivedController.Inherited), logger);
        var dto = Ops.RunController<DerivedController>(nameof(DerivedController.InheritedDto), logger);

        Assert.True(inherited.Responses!.ContainsKey("409"));
        Assert.True(inherited.Responses.ContainsKey("500"));
        Assert.False(dto.Responses!.ContainsKey("409"));
        Assert.Contains(logger.Entries, e => e.Message.Contains("DerivedController.InheritedDto"));
    }

    [Fact]
    public void Without_a_logger_the_declarations_are_still_skipped()
    {
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var method = Ops.Method<MixedController>(nameof(MixedController.Dto));

        OperationProcessor.ApplyProblemResultAttributes(operation, method, Ops.Json, new EndpointScope(typeof(MixedController), null));

        Assert.Empty(operation.Responses);
    }
}
