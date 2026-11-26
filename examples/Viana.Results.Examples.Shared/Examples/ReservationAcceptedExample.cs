using Viana.Results.Examples.Shared.Catalog;
using Viana.Results.OpenApi;

namespace Viana.Results.Examples.Shared.Examples;

/// <summary>A reservation accepted for later processing (202).</summary>
public sealed class ReservationAcceptedExample : IExampleProvider<Result<Reservation>>
{
    public Result<Reservation> GetExample() => new(new Reservation(1, "R-0001"), 202);
}
