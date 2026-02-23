## Viana.Results (Core)

**Core** library of `Viana.Results` to standardize operation responses
using the **Result** pattern, with support for collections and pagination.

---

### NuGet package

```bash
dotnet add package Viana.Results
```

### Supported frameworks

* `netstandard2.0`
* `net5.0`
* `net8.0`
* `net10.0`

---

### Basic Result pattern usage

```csharp
using Viana.Results;

// Simple success
public Result ProcessData()
{
    return Results.Success("Operation completed successfully");
}

// Success with data
public Result<User> GetUser(int id)
{
    var user = database.FindUser(id);
    return user; // Implicit conversion
}

// Error
public Result ValidateInput(string input)
{
    if (string.IsNullOrEmpty(input))
        return Results.Failure(422, "Input cannot be empty");

    return Results.Success("Validation passed");
}

// Different error types
public Result ProcessRequest()
{
    // Not Found (404)
    return Results.NotFound("Resource not found");

    // Bad Request (400)
    // return Results.BadRequest("Invalid request");

    // Unauthorized (401)
    // return Results.Unauthorized("Unauthorized access");

    // Forbidden (403)
    // return Results.Forbidden("Forbidden access");

    // Conflict (409)
    // return Results.Conflict("Conflict detected");

    // Business Rule (400)
    // return Results.BusinessRuleViolated("Business rule violated");
}
```

---

### Validation

```csharp
using Viana.Results;

public Result ValidateUser(User user)
{
    var errors = new Dictionary<string, string[]>
    {
        ["Email"] = new[] { "Email is required", "Invalid email" },
        ["Password"] = new[] { "Password must be at least 8 characters" }
    };

    return Results.Validation(errors, "Validation failed");
}
```

---

### Collections and pagination

```csharp
using Viana.Results;

// Simple collection
public CollectionResult<Product> GetProducts()
{
    var products = database.GetProducts().ToList();
    return products; // Implicit conversion
}

// Paginated result
public PaginatedResult<Product> GetProductsPaged(int page, int pageSize)
{
    var query = database.GetProducts();
    var total = query.Count();
    var items = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    var pages = (int)Math.Ceiling(total / (double)pageSize);

    return new PaginatedResult<Product>(items, total, pages);
}
```

---

### Available methods in `Results`

Some of the most important methods of the static `Results` class:

| Method                                                                                                   | Status Code | Description                        |
| -------------------------------------------------------------------------------------------------------- | ----------- | ---------------------------------- |
| `Success(string message = "Ok")`                                                                         | 200         | Successful operation with message  |
| `Success(object data)`                                                                                   | 200         | Successful operation with data     |
| `Success(string message, object data)`                                                                   | 200         | Success with message and data      |
| `Failure(string message, HttpStatusCode statusCode)`                                                     | Custom      | Generic failure with custom status |
| `Failure(ResultError error, string message = null, object data = null, HttpStatusCode statusCode = 422)` | 422         | Failure with error details         |
| `Failure(Exception exception, HttpStatusCode statusCode = 500)`                                          | 500         | Failure from exception             |
| `NotFound(string message = "The requested resource was not found.")`                                     | 404         | Resource not found                 |
| `BadRequest(string message = "Bad request", object data = null)`                                         | 400         | Invalid request                    |
| `Unauthorized(string message = "Unauthorized access.")`                                                  | 401         | Unauthorized                       |
| `Forbidden(string message = "Forbidden access.")`                                                        | 403         | Forbidden                          |
| `Conflict(string message = "Conflict occurred.")`                                                        | 409         | Conflict                           |
| `BusinessRuleViolated(string message, object data = null)`                                               | 422         | Business rule violation            |
| `Validation(Dictionary<string, string[]> errors, string message = "Validation failed")`                  | 400         | Validation error (arrays)          |
| `Validation(Dictionary<string, List<string>> errors, string message = "Validation failed")`              | 400         | Validation error (lists)           |
