# Quickstart: validate

1. `dotnet build -c Release`: expect 0 warnings.
2. `dotnet test`: all tests pass, including `DefaultResultTests` (both generators, both styles) and the updated
   `Results.Ok()` assertions (204).
3. `dotnet run --project examples/Viana.Results.Scalar` and open `/openapi/v1.json`:
   - `PUT`/`DELETE` on `/api/products/{id}` and `/minimal/products/{id}`, and `POST /api/inventory/availability`,
     list **204** and no 200, without `Produces(204)` in code;
   - the `WithoutDefaultResult` sample endpoints list only their hand-registered successes (200 and 202).
4. Repeat on `examples/Viana.Results.Swagger` (`/swagger/v1/swagger.json`).
5. `GET /api/samples/ok` answers **204**.
