namespace Viana.Results.Examples.Shared.Catalog;

/// <summary>Input validation: one validation problem listing every invalid field.</summary>
public static class ProductRules
{
    /// <summary>Validates <paramref name="input"/>; returns a success or a validation problem.</summary>
    public static Result Validate(ProductInput? input)
    {
        if (input is null)
            return Results.BadRequest("A product body is required.");

        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(input.Sku))
            Add(errors, "sku", "The SKU is required.");
        else if (input.Sku.Length is < 3 or > 20)
            Add(errors, "sku", "The SKU must have between 3 and 20 characters.");

        if (string.IsNullOrWhiteSpace(input.Name))
            Add(errors, "name", "The name is required.");
        else if (input.Name.Length > 100)
            Add(errors, "name", "The name must have at most 100 characters.");

        if (input.Price <= 0)
            Add(errors, "price", "The price must be greater than zero.");

        if (input.Stock < 0)
            Add(errors, "stock", "The stock cannot be negative.");

        // Results.Validation builds the RFC 9457 validation problem with an "errors" member.
        return errors.Count == 0 ? Results.Ok() : Results.Validation(errors);
    }

    private static void Add(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.TryGetValue(field, out var list))
            errors[field] = list = [];
        list.Add(message);
    }
}
