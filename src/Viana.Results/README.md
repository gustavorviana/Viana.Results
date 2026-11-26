## Viana.Results

Result pattern with HTTP-aligned statuses and RFC 9457 problem details. Runs anywhere .NET runs.

```bash
dotnet add package Viana.Results
```

Targets: `netstandard2.0`, `net8.0`, `net10.0`.

### Usage

```csharp
using Viana.Results;

public Result<User> Get(int id)
{
    var user = repository.Find(id);
    if (user is null)
        return Results.NotFound("User not found");   // 404 + problem

    return user;                                     // 200 with the user
}

public Result Delete(int id) => Results.NoContent(); // 204
```

### Common helpers

| Method | Status |
|---|---|
| `Results.Ok()` / `Results.NoContent()` | 204 |
| `Results.Ok(data)` | 200 |
| `Results.Created(data)` | 201 |
| `Results.BadRequest()` / `Results.Validation(errors)` | 400 |
| `Results.Unauthorized()` / `Results.Forbidden()` | 401 / 403 |
| `Results.NotFound()` / `Results.Conflict()` | 404 / 409 |
| `Results.BusinessRuleViolated(message)` | 422 |

### Lists and pages

```csharp
public ListResult<Product> All() => repository.GetAll().ToList();          // [ ... ]

public PagedResult<Product> Page(int page, int size)
    => PagedResult<Product>.Create(items, page, size, total);              // { data, pageNumber, totalPages }
```

### Your own list result (envelope)

```csharp
public sealed class CursorResult<T>(IReadOnlyList<T> data, string? nextCursor) : IEnvelopeResult<IReadOnlyList<T>>
{
    public int Status => 200;
    public ProblemResult? Problem => null;
    public IReadOnlyList<T> Data => data;
    public string? NextCursor => nextCursor;
}
// body: { "data": [...], "nextCursor": "abc" }
```

### Richer problems

```csharp
return new ProblemBuilder(404)
    .WithTitle("User not found")
    .WithDetail("No user with id 42.")
    .AddExtension("traceId", traceId)
    .BuildResult<User>();
```

Examples: [examples](https://github.com/gustavorviana/Viana.Results/tree/main/examples).
