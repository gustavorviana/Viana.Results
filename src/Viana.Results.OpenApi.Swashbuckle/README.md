## Viana.Results.OpenApi.Swashbuckle

`Swashbuckle.AspNetCore` integration for `Viana.Results`. Plugs filters into
`SwaggerGen` so the generated OpenAPI document reflects the actual response
shape: `Result<T>` → schema of `T`, `ListResult<T>` → array of `T`,
`PagedResult<T>` → kept wrapped (paging metadata preserved), errors as RFC 9457.

Requires Swashbuckle 10.x+ (post `Microsoft.OpenApi` 3.x).

---

### Install

```bash
dotnet add package Viana.Results.OpenApi.Swashbuckle
```

Target framework: `net10.0`.

### Register

```csharp
using Viana.Results.OpenApi.Swashbuckle;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddVianaResultFilters();
});
```

This single call wires up:

| Filter | What it does |
|---|---|
| `OmitProblemSchemaFilter` | Removes `status`/`problem` wrapper props from any `IResult` schema |
| `NoResponseBodyOperationFilter` | Clears response content when the action returns the bare `Result` |
| `RemoveResultSchemaDocumentFilter` | Drops the bare `Result` schema from components |
| `UnwrapResultFilter` | Replaces `Result<T>`/`ListResult<T>` response schemas with the payload schema |
| `ProblemResponseOperationFilter` | Emits RFC 9457 problem responses declared via `[ProblemResult(status)]` |
| `UnauthorizedAndForbiddenOperationFilter` | Auto-adds 401/403 problem responses to `[Authorize]` endpoints |
| `ResponseExampleOperationFilter` | Materializes `[ResponseExample]` and globally-registered examples |

---

### Documenting error responses

```csharp
using Viana.Results.OpenApi;

[HttpGet("{id}")]
[ProblemResult(404, Title = "User not found")]
[ProblemResult(409, Title = "Conflict")]
public Result<User> Get(int id) => repository.Find(id);
```

### Typed response examples

Define a provider implementing `IExampleProvider<T>`:

```csharp
using Viana.Results;
using Viana.Results.OpenApi;

public sealed class UserNotFoundExample : IExampleProvider<ProblemResult>
{
    public ProblemResult GetExample() => new ProblemBuilder(404)
        .WithTitle("User not found")
        .WithType("https://api.example.com/errors/user-not-found")
        .WithDetail("No user with the given id exists.")
        .AddExtension("traceId", "00-abc-...")
        .Build();
}
```

Attach per method, per controller, or globally:

```csharp
// per method (highest precedence)
[ResponseExample(404, typeof(UserNotFoundExample))]
public Result<User> Get(int id) { ... }

// per controller (applies to every action without its own override)
[ResponseExample(401, typeof(UnauthorizedExample))]
public class SecureController : ControllerBase { ... }

// global (applies when neither method nor controller cover the status)
builder.Services.AddVianaResultExamples(opts =>
{
    opts.AddExample<InternalServerErrorExample>(500, summary: "Generic 500");
});
```

Multiple `[ResponseExample]` with the same status + different `Name` produces
the OpenAPI 3 `examples` map; a single one (or all without `Name`) produces
the singular `example`.

---

### See also

* **`Viana.Results.OpenApi`** — same feature set wired as
  `Microsoft.AspNetCore.OpenApi` transformers (use this when you call
  `AddOpenApi()` directly, e.g. with Scalar).
* **`Viana.Results`** — the core package (`ProblemBuilder`, `Result<T>`,
  `ListResult<T>`, `PagedResult<T>`).
