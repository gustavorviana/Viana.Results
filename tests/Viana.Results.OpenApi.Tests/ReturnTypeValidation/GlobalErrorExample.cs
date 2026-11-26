namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

public sealed class GlobalErrorExample : IExampleProvider<ProblemResult>
{
    public ProblemResult GetExample() => new ProblemBuilder(500).WithTitle("Boom").Build();
}
