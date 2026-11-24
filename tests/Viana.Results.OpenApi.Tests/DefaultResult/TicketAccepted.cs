namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>202 with a ticket.</summary>
public sealed class TicketAccepted : IExampleProvider<Result<Ticket>>
{
    public Result<Ticket> GetExample() => new Result<Ticket>(new Ticket("T-1"), 202);
}
