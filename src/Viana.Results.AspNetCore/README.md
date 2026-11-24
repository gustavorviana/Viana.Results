## Viana.Results.AspNetCore

ASP.NET Core integration for [Viana.Results](../Viana.Results/README.md) with **native ProblemDetails
interop**: failures go through the framework's own problem details pipeline and successes through
normal MVC formatting. Successor of `Viana.Results.Mvc` for net8.0 and net10.0.

### Install

```bash
dotnet add package Viana.Results.AspNetCore
```

Targets: `net8.0`, `net10.0`.

### Usage

```csharp
builder.Services.AddProblemDetails();          // recommended: native defaults + CustomizeProblemDetails
builder.Services.AddControllers()
    .AddVianaResultFilter();                   // optional: return Result/Result<T> directly from actions
```

```csharp
using Viana.Results.AspNetCore;

[HttpGet("{id}")]
public IActionResult Get(int id) => _service.GetUser(id).ToActionResult();

[HttpGet("async/{id}")]
public Task<IActionResult> GetAsync(int id) => _service.GetUserAsync(id).ToActionResult();

// With AddVianaResultFilter():
[HttpGet("direct/{id}")]
public Result<User> GetDirect(int id) => _service.GetUser(id);
```

### Failures (RFC 9457)

A failed result (`Result.Problem` set) is written as a native `ProblemDetails` with content type
`application/problem+json`:

1. **Viana values first**: status, title, type, `detail`, `instance` and extension members you set.
2. **Native defaults fill what is missing**: e.g. the RFC 9110 `type` link per status when the Viana
   type is `about:blank`.
3. **`CustomizeProblemDetails` runs last** and can change anything.

Rules:

- The HTTP status and the `status` member are always `Result.Status`.
- **`traceId` always comes from ASP.NET Core** (`Activity.Current?.Id ?? HttpContext.TraceIdentifier`,
  the same value the framework uses). A `traceId` extension set in Viana is only used when the native
  pipeline does not provide one. This is identical on net8.0 and net10.0.
- Extension member names are case-insensitive and written with the MVC JSON naming policy
  (`JsonOptions.JsonSerializerOptions.PropertyNamingPolicy`, camelCase by default): `TraceId` → `traceId`,
  `ErrorCode` → `errorCode`. Names that collide after conversion keep the last value.
- `detail` and `instance` become the standard members; the obsolete `description` stays an extension.
- Works with and without `AddProblemDetails()` (without it, the native `ProblemDetailsFactory` is used).
- Newtonsoft: extension names are written as stored (a dedicated Newtonsoft package is planned).

### Successes

- `Results.Ok()` without a value is **204 No Content** with no body; a bodyless result keeps its own status
  (`new Result(200)` answers 200).

Success payloads are written with `ObjectResult`, so content negotiation and the configured formatters
(System.Text.Json, `AddNewtonsoftJson()`, `AddXmlSerializerFormatters()`) apply, with `Result.Status`:

- `ListResult<T>` → the items array; `PagedResult<T>` → wrapper with paging metadata.
- No value (e.g. `204`, non-generic `Result`) → no body.
- **A null value follows the MVC default**: 204 without body, unless the app sets
  `HttpNoContentOutputFormatter.TreatNullValueAsNoContent = false` (then `null` with the result status).
  This differs from `Viana.Results.Mvc`, which writes `null` with the result status.

### Minimal APIs

```csharp
using Viana.Results.AspNetCore;

app.MapGet("/users/{id}", (int id) => service.GetUser(id).ToHttpResult());

// Or let handlers return Viana results directly (endpoint or group):
var users = app.MapGroup("/users").AddVianaResults();
users.MapGet("/{id}", (int id) => service.GetUser(id));
users.MapGet("/async/{id}", async (int id) => await service.GetUserAsync(id));
```

Same rules as MVC: failures through the native problem details pipeline (native `traceId`, extension
names with the minimal API JSON naming policy `Microsoft.AspNetCore.Http.Json.JsonOptions`), successes as
JSON with the result status. A null value answers 204, like the MVC default, so controllers and minimal
APIs answer identically when both JSON options are configured the same way.

### Migrating from Viana.Results.Mvc

1. Replace the package `Viana.Results.Mvc` with `Viana.Results.AspNetCore`.
2. Replace `using Viana.Results.Mvc;` with `using Viana.Results.AspNetCore;`.
3. Optionally add `builder.Services.AddProblemDetails();`.

`AddVianaResultFilter()` keeps its name. Differences: failures use the native pipeline (native `type`
links, `traceId`, `CustomizeProblemDetails`), successes use MVC formatters, a null value follows the MVC
default, and `VianaResultAction`'s `Headers`/`ContentType` extension points are not carried over (set
headers through the normal framework mechanisms).

### Performance

Going through the native pipeline costs a little more than the old hand-written JSON (see
[benchmarks](../../benchmarks/Viana.Results.Benchmarks/baseline/aspnetcore-1.0.0.md)): content
negotiation, MVC formatters and `IProblemDetailsService` are what give the native interop.

---

### Examples

Runnable examples for every package: [examples](https://github.com/gustavorviana/Viana.Results/tree/main/examples).
