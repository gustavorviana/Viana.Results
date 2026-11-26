# Changelog

Versions are managed by release-please from Conventional Commits. The entries below were written
before release-please was adopted (feature 017) and are not yet released.

## Unreleased (since Viana.Results.Mediators 1.0.0)

- 1.1.0
- Performance: process-wide typed dispatch (no reflection after the first call, no closure, no extra async layer); steady-state dispatch allocates only what the handler does.
- Fixed: request implementing two IRequest<> contracts reached the wrong handler; handler returning a null task; HandlerNotFoundException named the handler contract instead of the request (new RequestType/ResultType properties); missing handler left the span without error status; repeated AddMediator duplicated registrations; assembly scanning failed on types that cannot be loaded and registered open generic handlers.
- Changed: two different handlers for the same request and result type now fail at registration.
- Changed (telemetry tags): request.type, result.type, handler.type, result.success and result.Status became viana.mediator.request.type, viana.mediator.result.type, viana.mediator.handler.type (implementation type name), viana.mediator.result.success and viana.mediator.result.status; plus error.type and viana.mediator.cancelled.
- Added: AddMediator(options, assemblies) with Telemetry (Tracing, Metrics, ExceptionDetails: Full, TypeOnly, None); duration histogram viana.mediator.request.duration; MediatorDiagnostics name constants; cancellation is not an error; 5xx problem results set Error status; native exception recording on .NET 9+.
- 1.0.0
- Provides IRequest<T>, IHandler<TRequest, TResponse> and IMediator for a minimal Mediator pattern integrated with Viana.Results.
- The telemetry status of Results.Ok() without a value is 204 (via Viana.Results).
- Added: MediatorDiagnostics.DurationBucketBoundaries (seconds-based buckets 0.005 to 10 s), advised on the duration histogram on .NET 9+; on net6.0/net8.0 configure the buckets with an OpenTelemetry View.
