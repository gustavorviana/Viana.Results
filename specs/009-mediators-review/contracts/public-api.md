# Public API Contract: Viana.Results.Mediators 1.1.0

Unchanged (FR-014): `IRequest<TResult>`, `IHandler<TRequest, TResult>.Handle`, `IMediator.SendAsync`,
`AddMediator(Assembly)`, `AddMediator(params Assembly[])`, `Mediator(IServiceProvider)`.

Added:

```csharp
namespace Viana.Results.Mediators;

public sealed class MediatorOptions
{
    public ServiceLifetime HandlerLifetime { get; set; } = ServiceLifetime.Scoped;
    public MediatorTelemetryOptions Telemetry { get; } = new();
}

public sealed class MediatorTelemetryOptions
{
    public bool Tracing { get; set; } = true;
    public bool Metrics { get; set; } = true;
    public ExceptionDetailLevel ExceptionDetails { get; set; } = ExceptionDetailLevel.Full;
}

public enum ExceptionDetailLevel { Full, TypeOnly, None }

public static class MediatorDiagnostics
{
    public const string SourceName = "Viana.Results.Mediators";
    public const string MeterName = "Viana.Results.Mediators";
    public const string DurationInstrument = "viana.mediator.request.duration";
    // tag name constants: RequestTypeTag, ResultTypeTag, HandlerTypeTag, SuccessTag, StatusTag, CancelledTag
}

public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorOptions> configure, params Assembly[] assemblies);

public class Mediator
{
    public Mediator(IServiceProvider serviceProvider, MediatorOptions options);
}

public class HandlerNotFoundException
{
    public HandlerNotFoundException(Type requestType, Type resultType);
    public Type RequestType { get; }
    public Type? ResultType { get; }
}
```

Behavior changes (approved, release notes): tags renamed to `viana.mediator.*`; duplicate handlers for the
same request/result throw at registration; repeated `AddMediator` calls no longer duplicate services.
