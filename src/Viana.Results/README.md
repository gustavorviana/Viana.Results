## Viana.Results (Core)

Standardized operation responses with the **Result** pattern, including
collections, pagination, and RFC 9457 problem details.

This is the core package. ASP.NET Core MVC integration lives in
`Viana.Results.Mvc`. OpenAPI/Swagger integration lives in
`Viana.Results.OpenApi` (native) and `Viana.Results.OpenApi.Swashbuckle`.

---

### Install

```bash
dotnet add package Viana.Results
```

### Target frameworks

`netstandard2.0`, `net8.0`, `net10.0`

---

### Basic usage

```csharp
using Viana.Results;

// Success with no body (HTTP 204)
public Result Delete(int id) => Results.NoContent();

// Success with typed payload (HTTP 200)
public Result<User> GetUser(int id)
{
    var user = repository.Find(id);
    return user;                  // implicit User -> Result<User>
}

// Failure (problem details body, status code from problem)
public Result<User> Get(int id)
{
    var user = repository.Find(id);
    if (user is null)
        return Results.NotFound("User not found");   // implicit ProblemResult -> Result<User>

    return user;
}
```

### Factory helpers (`Results` static class)

| Method | Status | Notes |
|---|---|---|
| `Results.Ok()` | 200 | Empty success |
| `Results.Ok(string message)` | 200 | Returns `Result<string>` |
| `Results.Ok<T>(T data)` | 200 | Typed success |
| `Results.Created<T>(T data)` | 201 | |
| `Results.NoContent()` | 204 | |
| `Results.BadRequest(string? title = null)` | 400 | |
| `Results.Unauthorized(string? title = null)` | 401 | |
| `Results.Forbidden(string? title = null)` | 403 | |
| `Results.NotFound(string? title = null)` | 404 | |
| `Results.Conflict(string? title = null)` | 409 | |
| `Results.BusinessRuleViolated(string? message)` | 422 | |
| `Results.Validation(Dictionary<string, string[]> errors, string? title = null)` | 400 | Errors land under `extensions.errors` |
| `Results.Failure(HttpStatusCode status, string? title = null)` | custom | |
| `Results.Failure(Exception ex, HttpStatusCode status = 500)` | custom | |

---

### Lists and pagination

```csharp
// ListResult<T> — serialized as a plain JSON array
public ListResult<Product> GetAll()
{
    return repository.GetAll().ToList();   // implicit List<T> -> ListResult<T>
}

// PagedResult<T> — serialized as { "data": [...], "pageNumber": N, "totalPages": N }
public PagedResult<Product> GetPage(int page, int pageSize)
{
    var items = repository.Query()
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    var total = repository.Count();
    return PagedResult<Product>.Create(items, page, pageSize, total);
}
```

> **2.0 breaking change:** `ListResult<T>` now serializes as a bare JSON array
> (not as `{ "data": [...] }`). `PagedResult<T>` keeps its wrapper because of
> the paging metadata. Clients reading the previous shape need to be updated.

---

### Problem details (RFC 9457)

```csharp
// Quick helper for validation
return Results.Validation(new Dictionary<string, string[]>
{
    ["email"]    = new[] { "is required", "must be a valid email" },
    ["password"] = new[] { "must be at least 8 characters" }
});
```

### Fluent ProblemBuilder

For richer problem payloads (custom type URI, RFC 9457 `detail` /
`instance`, extension members like `traceId`):

```csharp
using Viana.Results;

var problem = new ProblemBuilder(404)
    .WithTitle("User not found")
    .WithType("https://api.example.com/errors/user-not-found")
    .WithDetail("No user with id '8f3e2a1b' exists in tenant 'acme'.")
    .WithInstance("/api/users/8f3e2a1b")
    .AddExtension("traceId", Activity.Current?.TraceId.ToString())
    .Build();

return new Result(problem);
```

`WithDetail` and `WithInstance` write to the RFC 9457 `detail` / `instance`
keys under `Extensions`. Reserved members (`type`, `title`, `status`,
`extensions`) added via `AddExtension` are silently filtered out by
`ProblemResult`.

---

### Validation

```csharp
public Result ValidateUser(User user)
{
    var errors = new Dictionary<string, string[]>
    {
        ["email"]    = new[] { "is required" },
        ["password"] = new[] { "must be at least 8 characters" }
    };

    return Results.Validation(errors, "Validation failed");
}
```

Serializes the errors under `extensions.errors`, matching the
[Microsoft.AspNetCore.Mvc.ValidationProblemDetails](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.validationproblemdetails)
shape.

---

### Companion packages

* **`Viana.Results.Mvc`** — a result filter that converts any `IResult` into a native
  ASP.NET Core response (mapping errors to `ProblemDetails`) with the correct HTTP status.
* **`Viana.Results.Mediators`** — minimalist mediator that returns `IResult`-typed values.
* **`Viana.Results.OpenApi`** — native `Microsoft.AspNetCore.OpenApi` transformers
  (unwrap, problem responses, `[ResponseExample]`, etc.).
* **`Viana.Results.OpenApi.Swashbuckle`** — same features wired up as
  `Swashbuckle.AspNetCore` filters.
