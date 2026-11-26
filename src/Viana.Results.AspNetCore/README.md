## Viana.Results.AspNetCore

Returns [Viana.Results](../Viana.Results/README.md) from ASP.NET Core controllers and minimal APIs. Failures go
through the native ProblemDetails pipeline.

```bash
dotnet add package Viana.Results.AspNetCore
```

Targets: `net8.0`, `net10.0`.

### Controllers

```csharp
builder.Services.AddProblemDetails();
builder.Services.AddControllers().AddVianaResultFilter();

[HttpGet("{id}")]
public Result<User> Get(int id) => _service.GetUser(id);

// Without the filter:
[HttpGet("other/{id}")]
public Task<IActionResult> GetOther(int id) => _service.GetUserAsync(id).ToActionResult();
```

### Minimal APIs

```csharp
var users = app.MapGroup("/users").AddVianaResults();
users.MapGet("/{id}", (int id) => service.GetUser(id));

// Or per endpoint:
app.MapGet("/me", () => service.Me().ToHttpResult());
```

Failures are written as `application/problem+json` with the native `traceId`, and `CustomizeProblemDetails`
still runs. Successes use the normal formatters with the result's status.

Examples: [examples](https://github.com/gustavorviana/Viana.Results/tree/main/examples).
