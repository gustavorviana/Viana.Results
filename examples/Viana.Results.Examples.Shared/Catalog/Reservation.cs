namespace Viana.Results.Examples.Shared.Catalog;

/// <summary>A stock reservation accepted for later processing (202).</summary>
/// <param name="ProductId">The reserved product.</param>
/// <param name="Code">Code to follow the reservation.</param>
public sealed record Reservation(int ProductId, string Code);
