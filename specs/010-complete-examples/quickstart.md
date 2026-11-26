# Quickstart: validate the examples

1. `dotnet build -c Release`: 0 warnings, and no example project is packable (`dotnet pack` skips them).
2. `dotnet test tests/Viana.Results.Examples.Tests`: every route and status in
   [contracts/endpoints.md](contracts/endpoints.md) passes on each host.
3. `dotnet run --project examples/Viana.Results.Scalar`: `/` redirects to Scalar. Run `catalog.http`; every
   documented status appears in the UI for each operation.
4. Do the same with `examples/Viana.Results.Swagger` (Swagger UI).
5. `dotnet run --project examples/Viana.Results.Mediators.Example`, then run `mediators.http`. The console
   shows the mediator spans (tags `viana.mediator.*`) and the `viana.mediator.request.duration` metric.
   Set `Mediator:Telemetry:ExceptionDetails=TypeOnly` and call `/scenarios/throws`: the exception event has
   no message. Set `Tracing=false`: no spans are printed.
6. The README links resolve, and no other mediator library is named in examples/ or this spec folder.
