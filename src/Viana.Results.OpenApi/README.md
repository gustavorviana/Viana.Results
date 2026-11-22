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
| `UnauthorizedAndForbiddenOperationTransformer` | Auto-adds 401/403 problem responses to endpoints requiring authorization (`[Authorize]`, `RequireAuthorization()`), except `[AllowAnonymous]` / `AllowAnonymous()` |
| `ResponseExampleOperationTransformer` | Materializes `[ResponseExample]` and globally-registered examples |

---

### Documenting error responses

```csharp
[HttpGet("{id}")]
[ProblemResult(404, Title = "User not found")]
[ProblemResult(409, Title = "Conflict", Detail = "The user already exists.")]
public Result<User> Get(int id) => repository.Find(id);
```

`Detail` is documented as the RFC 9457 `detail` member of the problem schema (with the text as
example). `Description` is obsolete: it never had an effect and will be removed in
Viana.Results.OpenApi 2.0.0.

Actions returning `Task<T>` or `ValueTask<T>` are handled the same way.

### How problem responses are documented

- Every problem response uses the content type `application/problem+json` and references a shared
  `VianaProblemDetails` component with the native `ProblemDetails` members (`type`, `title`, `status`,
  `detail`, `instance`; optional and nullable). Responses with extra members use
  `allOf: [VianaProblemDetails, { their members }]`.
- For each status, schema and example come from **one** source, in this order:

  | Priority | Source |
  |----------|--------|
  | 1 | `[ResponseExample]` on the endpoint (several examples are merged into one schema) |
  | 2 | `[ProblemResult]` on the action |
  | 3 | `[ProblemResult]` on the controller |
  | 4 | global example from `AddVianaResultExamples` (fallback only) |

- Extension member names follow the application JSON naming policy (camelCase by default), like the
  runtime response.

### Minimal API endpoints

```csharp
app.MapGet("/users/{id}", handler)
   .ProducesResult<UserOkExample>()                 // example class returning Result<User>: status/example from it
   .ProducesProblemResult<UserNotFoundExample>();   // example class returning a failed Result

app.MapGet("/users", handler)
   .ProducesResult<User[]>()                        // data type only (200)
   .ProducesProblemResult(409, title: "Conflict");  // problem metadata

// Inline values, for those who prefer not to create classes:
app.MapGet("/me", handler)
   .ProducesResult(Results.Ok(new User { Name = "Ana" }))
   .ProducesProblemResult(new ProblemBuilder(401).WithTitle("Login required").BuildResult());
```

- `ProducesResult<T>()`: `T` is an example class when it implements `IExampleProvider<>`, otherwise the
  documented data type.
- When an example is given, the status comes from its result; an explicit `status:` that differs is an
  error at document generation.
- Same per-status precedence as controllers (endpoint examples > endpoint declarations > global example).

### Example classes returning real results

Example providers may return the real Viana result (`IExampleProvider<Result<User>>`, `ListResult<T>`,
`PagedResult<T>`, a failed `Result`): the example is written exactly like the runtime body (value
unwrapped, list as array, paged with metadata, failure as problem) and the status comes from the result.
On controllers, use `[ResponseExample(typeof(Provider))]` without status (`StatusCode` is then `0`).

A documented problem without its own type (null, empty or `about:blank`) shows the type the runtime sends:
the ASP.NET Core RFC 9110 link for the statuses it covers (400, 401, 403, 404, 405, 406, 408, 409, 412, 415,
422, 426, 500, 502, 503, 504), `about:blank` otherwise. Explicit types are never changed.

### Typed response examples

Example providers are resolved from DI or created with their constructor dependencies injected;
`AddVianaResultExamples` can be called more than once (registrations are combined).

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
