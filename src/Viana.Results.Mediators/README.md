## Viana.Results.Mediators

Small, fast mediator: requests go to handlers that return a Viana `Result`, with built-in traces and metrics.

```bash
dotnet add package Viana.Results.Mediators
```

Targets: `netstandard2.0` (no telemetry), `net6.0`, `net8.0`, `net10.0`.

### Request and handler

```csharp
public class GetUser : IRequest<Result<User>>
{
    public int Id { get; set; }
}

public class GetUserHandler(IUserRepository repository) : IHandler<GetUser, Result<User>>
{
    public async Task<Result<User>> Handle(GetUser request, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindAsync(request.Id, cancellationToken);
        return user is null ? Results.NotFound("User not found") : user;
    }
}
```

### Register and send

```csharp
services.AddMediator(typeof(GetUserHandler).Assembly);

var result = await mediator.SendAsync(new GetUser { Id = 42 });
```

Handlers are registered as scoped. To use another lifetime, register the handler yourself before calling
`AddMediator`.

### Telemetry (OpenTelemetry)

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddSource("Microsoft.AspNetCore")           // needed so mediator spans are kept
        .AddSource(MediatorDiagnostics.SourceName))
    .WithMetrics(m => m.AddMeter(MediatorDiagnostics.MeterName));
```

Each dispatch creates a `Mediator.Send {Request}` span and records the `viana.mediator.request.duration`
histogram, in seconds. Use `o.Telemetry.ExceptionDetails` to choose how much exception data is recorded.

Examples: [examples](https://github.com/gustavorviana/Viana.Results/tree/main/examples).
