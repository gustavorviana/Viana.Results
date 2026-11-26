namespace Viana.Results.OpenApi.Tests.EmptyResponses;

public sealed class EmptyProblemExample : IExampleProvider<ProblemResult>
{
    public ProblemResult GetExample() => new ProblemBuilder(404).WithTitle("Missing").Build();
}
