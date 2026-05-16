## Viana.Results.OpenApi

Native `Microsoft.AspNetCore.OpenApi` integration for `Viana.Results`.
**No Swashbuckle dependency.** Use this when you generate the OpenAPI
document via `services.AddOpenApi()` + `app.MapOpenApi()` (typical setup
with Scalar, Redoc, or `Swashbuckle.AspNetCore.SwaggerUI` consuming
`openapi.json`).

For Swashbuckle's `SwaggerGen` pipeline see
`Viana.Results.OpenApi.Swashbuckle`.

---

### Install

```bash
dotnet add package Viana.Results.OpenApi
```

Target framework: `net10.0`.

### Register

```csharp
using Viana.Results.OpenApi;

builder.Services.AddOpenApi(options =>
{
    options.AddVianaResultTransformers();
});

var app = builder.Build();
app.MapOpenApi();   // exposes /openapi/v1.json
```

This wires up the equivalent of every Swashbuckle filter:

| Transformer | What it does |
|---|---|
| `OmitProblemSchemaTransformer` | Removes `status`/`problem` wrapper props from any `IResult` schema |
| `NoResponseBodyOperationTransformer` | Clears response content when the action returns the bare `Result` |
| `RemoveResultSchemaDocumentTransformer` | Drops the bare `Result` schema from components |
| `UnwrapResultSchemaTransformer` | Rewrites `Result<T>`/`ListResult<T>` wrapper schemas as `allOf:[dataSchema]` |
| `ProblemResponseOperationTransformer` | Emits RFC 9457 problem responses declared via `[ProblemResult(status)]` |
| `UnauthorizedAndForbiddenOperationTransformer` | Auto-adds 401/403 problem responses to `[Authorize]` endpoints |
| `ResponseExampleOperationTransformer` | Materializes `[ResponseExample]` and globally-registered examples |

---

### Documenting error responses

```csharp
[HttpGet("{id}")]
[ProblemResult(404, Title = "User not found")]
[ProblemResult(409, Title = "Conflict")]
public Result<User> Get(int id) => repository.Find(id);
```

### Typed response examples

Same API as the Swashbuckle integration. Define a provider:

```csharp
using Viana.Results;
using Viana.Results.OpenApi;

public sealed class UserNotFoundExample : IExampleProvider<ProblemResult>
{
    public ProblemResult GetExample() => new ProblemBuilder(404)
        .WithTitle("User not found")
        .WithDetail("No user with the given id exists.")
        .AddExtension("traceId", "00-abc-...")
        .Build();
}
```

Attach per method, per controller, or globally:

```csharp
[ResponseExample(404, typeof(UserNotFoundExample))]
public Result<User> Get(int id) { ... }

[ResponseExample(401, typeof(UnauthorizedExample))]
public class SecureController : ControllerBase { ... }

builder.Services.AddVianaResultExamples(opts =>
{
    opts.AddExample<InternalServerErrorExample>(500, summary: "Generic 500");
});
```

Precedence: **method** > **class** > **global**. Multiple `[ResponseExample]`
with the same status + different `Name` produce the OpenAPI 3 `examples`
map; a single one (or all without `Name`) produce the singular `example`.

---

### Sharing logic with Swashbuckle

The actual mutation logic lives in `Viana.Results.OpenApi.Processing`
(`OpenApiOperationProcessor`, `OpenApiSchemaProcessor`,
`OpenApiDocumentProcessor`). Both the transformers in this package and
the filters in `Viana.Results.OpenApi.Swashbuckle` are thin adapters over
these processors, so the OpenAPI document is identical regardless of
which pipeline emits it.
