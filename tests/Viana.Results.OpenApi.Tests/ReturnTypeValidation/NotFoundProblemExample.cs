namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

public sealed class NotFoundProblemExample : IExampleProvider<ProblemResult>
{
    public ProblemResult GetExample() => new ProblemBuilder(404).WithTitle("Missing").Build();
}
