# Data Model: Mediators Review

## MediatorOptions (public)

| Field | Type | Default | Rule |
|-------|------|---------|------|
| HandlerLifetime | ServiceLifetime | Scoped | applies to scanned handlers |
| Telemetry | MediatorTelemetryOptions | new() | not null |

## MediatorTelemetryOptions (public)

| Field | Type | Default | Rule |
|-------|------|---------|------|
| Tracing | bool | true | false → ActivitySource never consulted |
| Metrics | bool | true | false → Meter instruments never recorded |
| ExceptionDetails | ExceptionDetailLevel | Full | Full / TypeOnly / None (research R4) |

## HandlerWrapper (internal)

- `HandlerWrapper<TResult>`: abstract `Task<TResult> Handle(IRequest<TResult>, IServiceProvider, CancellationToken)`,
  `string RequestTypeName`, `string ResultTypeName`.
- `HandlerWrapper<TRequest, TResult>`: resolves `IHandler<TRequest, TResult>` (missing →
  `HandlerNotFoundException`), calls `Handle`, null task → `InvalidOperationException`. Exposes the resolved
  handler type name for the tag.
- Cached once per (request runtime type, result type) in `DispatchCache<TResult>`.

## Telemetry output (net6.0+)

| Item | Name | Kind |
|------|------|------|
| Source / Meter | `Viana.Results.Mediators` | constants in `MediatorDiagnostics` |
| Span | `Mediator.Send {RequestType}` | Internal |
| Tags | `viana.mediator.request.type`, `.result.type`, `.handler.type` (string); `.result.success` (bool); `.result.status` (int); `viana.mediator.cancelled` (bool, only when cancelled); `error.type` (string, failures) | span |
| Histogram | `viana.mediator.request.duration` (s) | dims: request.type, result.status, result.success, error.type |

## State/outcome rules

| Outcome | Span status | Exception event | Histogram |
|---------|-------------|-----------------|-----------|
| Success / 4xx problem | Ok | — | recorded with status |
| 5xx problem | Error (problem title) | — | recorded |
| Exception | Error | per detail level | recorded, `error.type` |
| Handler not found | Error | per detail level | recorded, `error.type` |
| Cancellation | Unset | — | recorded, `error.type` = exception type |
