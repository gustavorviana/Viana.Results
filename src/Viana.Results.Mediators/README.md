## Viana.Results.Mediators

A small, fast **mediator** built on `Viana.Results`: requests are dispatched to handlers that return a
`Result`, with built-in traces and metrics.

---

### NuGet package

```bash
dotnet add package Viana.Results.Mediators
```

### Supported frameworks

* `netstandard2.0` (dispatch only, no telemetry)
* `net6.0`, `net8.0`, `net10.0` (dispatch + traces and metrics)

---

### Defining a request and its handler

```csharp
using Viana.Results;
using Viana.Results.Mediators;

public class GetUserByIdRequest : IRequest<Result<User>>
{
    public int UserId { get; set; }
}

public class GetUserByIdHandler : IHandler<GetUserByIdRequest, Result<User>>
{
    private readonly IUserRepository _repository;

    public GetUserByIdHandler(IUserRepository repository) => _repository = repository;

    public async Task<Result<User>> Handle(GetUserByIdRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Results.NotFound("User not found");

        return user; // implicit conversion to Result<User>
    }
}
```

---

### Registering

```csharp
services.AddMediator(typeof(GetUserByIdHandler).Assembly);

// With options
services.AddMediator(o =>
{
    o.Telemetry.Tracing = true;                                  // default
    o.Telemetry.Metrics = true;                                  // default
    o.Telemetry.ExceptionDetails = ExceptionDetailLevel.Full;    // default
}, typeof(GetUserByIdHandler).Assembly, typeof(OtherHandler).Assembly);
```

Registration rules:

* Calling `AddMediator` more than once registers each service once; the **options of the first call win**.
* Abstract classes and open generic handlers are skipped; types that cannot be loaded are ignored.
* **Two different handlers for the same request and result type are an error** at registration.
* Handlers are resolved by the **exact** request type: a request deriving from another request does not
  use the base request's handler.
* Scanned handlers are registered as **scoped**.

#### Overriding a handler lifetime

Register the handler yourself **before** `AddMediator`; the scan keeps your registration (lifetime or
factory):

```csharp
services.AddSingleton<IHandler<GetCountriesRequest, ListResult<Country>>, GetCountriesHandler>();
services.AddMediator(typeof(GetCountriesHandler).Assembly);
```

A singleton handler cannot depend on scoped services (standard container validation applies).

---

### Sending (ASP.NET Core)

```csharp
using Viana.Results.AspNetCore;
using Viana.Results.Mediators;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
        => (await mediator.SendAsync(new GetUserByIdRequest { UserId = id })).ToActionResult();
}
```

Every dispatch error (null request, `HandlerNotFoundException`, a handler returning a null task, a handler
throwing) surfaces through the returned task, never at call time. `HandlerNotFoundException` exposes
`RequestType` and `ResultType`.

---

### Telemetry (.NET 6+)

The mediator uses the native .NET `ActivitySource` and `Meter`. Nothing is built unless a listener asks for
it, so telemetry is free when nobody collects it.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddSource(MediatorDiagnostics.SourceName))   // "Viana.Results.Mediators"
    .WithMetrics(m => m.AddMeter(MediatorDiagnostics.MeterName));    // "Viana.Results.Mediators"
```

In ASP.NET Core the mediator span is a child of the request span, and the default sampler follows the
parent: also record the request (`t.AddSource("Microsoft.AspNetCore")` or the ASP.NET Core
instrumentation), otherwise the mediator spans are dropped.

**Span** `Mediator.Send {RequestType}` (kind `Internal`, child of the current activity):

| Tag | Example |
|-----|---------|
| `viana.mediator.request.type` | `GetUserByIdRequest` |
| `viana.mediator.result.type` | `Result<User>` |
| `viana.mediator.handler.type` | `GetUserByIdHandler` |
| `viana.mediator.result.success` | `false` |
| `viana.mediator.result.status` | `404` |
| `viana.mediator.cancelled` | `true` (only when cancelled) |
| `error.type` | exception type (failures) |

**Status**: success and 4xx problems → `Ok`; 5xx problems → `Error` (problem title); exceptions and missing
handlers → `Error`; cancellation is not an error.

**Metric** `viana.mediator.request.duration` (histogram, seconds; its count is the number of dispatches),
dimensions `viana.mediator.request.type`, `viana.mediator.result.status`, `viana.mediator.result.success`
and `error.type` on failures.

**Exception details** (`Telemetry.ExceptionDetails`), since messages and stack traces may contain sensitive
data:

| Level | Recorded |
|-------|----------|
| `Full` (default) | type, message and stack trace |
| `TypeOnly` | exception type only |
| `None` | no exception event (status stays `Error`, `error.type` kept) |

**Turning it off**: `Telemetry.Tracing = false` and/or `Telemetry.Metrics = false`. A disabled kind is never
consulted, even with active listeners.

All names are available as constants in `MediatorDiagnostics`.

---

### Examples

Runnable examples for every package: [examples](https://github.com/gustavorviana/Viana.Results/tree/main/examples).
