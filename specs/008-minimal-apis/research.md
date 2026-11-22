# Research: Minimal APIs Support

## R1 – Shared writer
- **Decision**: Move the failure logic of `VianaActionResult` (mapper, native trace id, problem service or
  factory fallback) into internal `ResultResponseWriter.WriteProblemAsync(HttpContext, IResult, JsonNamingPolicy?, Func<ProblemDetails, Task> fallbackWriter)`.
  MVC fallback: `ProblemDetailsFactory` + `ObjectResult`; minimal fallback: `TypedResults.Problem(details)`
  (applies native defaults, writes `application/problem+json`).
- **Naming policy**: MVC → `Microsoft.AspNetCore.Mvc.JsonOptions`; minimal → `Microsoft.AspNetCore.Http.Json.JsonOptions`.

## R2 – Minimal success writing
- **Decision**: `!HasBody` → `TypedResults.StatusCode(status)`; otherwise `TypedResults.Json(body, (JsonSerializerOptions?)null, contentType: null, statusCode: status)`
  (uses `Http.Json.JsonOptions`).
- **Null value (FR-003 parity)**: MVC's default turns a null value into 204. Minimal APIs would write `null`
  with 200. To keep controller and minimal responses identical, minimal also returns **204 for a null
  value** (documented; matches the MVC default).

## R3 – Names
- `VianaHttpResult : Microsoft.AspNetCore.Http.IResult` (the Viana `IResult` is fully qualified inside).
- `ToHttpResult(this Viana.Results.IResult)` and `ToHttpResult<TResult>(this Task<TResult>)`.
- Filter: `VianaResultEndpointFilter : IEndpointFilter` (wraps a returned Viana result); registration
  `builder.AddVianaResults()` for `RouteHandlerBuilder` and `RouteGroupBuilder` (via
  `IEndpointConventionBuilder` + `AddEndpointFilter`).

## R4 – Endpoint documentation metadata
- **Decision**: `VianaResponseMetadata` (public, sealed) stored with `WithMetadata`, one per declaration:
  `Status?`, `DataType?`, `ExampleProviderType?`, `InlineExample?` (object), `Title?`, `Type?`, `Detail?`,
  `IsProblem`, `Name?`, `Summary?`.
- `ProducesResult<T>()`: if `T` implements `IExampleProvider<>` → provider; else data type `T` (status 200).
- `ProducesResult(object value)` / `ProducesProblemResult(object value)`: inline result; status from it.
- `ProducesProblemResult(int status, string? title, string? type, string? detail)`, `ProducesProblemResult<TProvider>()`,
  `ProducesProblemResult(Type providerType)`.
- Status mismatch (explicit status vs result status) → `InvalidOperationException` naming endpoint and
  statuses, raised at documentation time (providers may need DI, so the result is only known then).

## R5 – Reading metadata in both pipelines
- Native: `context.Description.ActionDescriptor.EndpointMetadata`; Swashbuckle:
  `context.ApiDescription.ActionDescriptor.EndpointMetadata`. Controllers keep attribute reading.
- Success schemas (owner decision, option C): the processor is async end to end and receives
  `Func<Type, ValueTask<IOpenApiSchema>>`. Native: `type => new(context.GetOrCreateSchemaAsync(type, null, ct))`.
  Swashbuckle: the Viana filters that generate schemas implement `IOperationAsyncFilter` (registered with
  `options.OperationAsyncFilter<T>()` inside `AddVianaResultFilters()`) and pass
  `type => ValueTask.FromResult(context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository))`
  (an already completed task: no waiting). No `.Result`/`.Wait()`/`GetAwaiter().GetResult()` anywhere.
- Synchronous Swashbuckle path (`ISwaggerProvider.GetSwagger`, used by some tools): **verified
  (2026-11-22): async filters are skipped there.** Outcome: the processor resolves declarations
  synchronously, generates the needed schemas (native: awaited `GetOrCreateSchemaAsync`; Swashbuckle:
  synchronous generator) and builds synchronously. Swashbuckle therefore keeps an ordinary synchronous
  `IOperationFilter`, which runs on both `GetSwagger` and `GetSwaggerAsync`. No code waits on async work
  (no `.Result`/`.Wait()`/`GetAwaiter().GetResult()` in the library).
- Precedence per status (feature 006 FR-014): endpoint examples (metadata example/inline or
  `[ResponseExample]`) > endpoint declarations (`ProducesProblemResult(status, ...)`, `[ProblemResult]`) >
  global example.

## R6 – Result-returning example providers
- `IExampleProvider<X>` where `X : IResult` → value is a Viana result: status = `result.Status`; body =
  `ResultResponseBody.GetBody(result)` (problem when failed); problem examples feed the shared problem schema.
- `[ResponseExample(Type providerType)]` (new ctor): status resolved from the result at documentation time;
  `[ResponseExample(int, Type)]` with a result provider whose status differs → error (FR-006).

## R7 – Versions
- Viana.Results.AspNetCore 1.0.0, Viana.Results.OpenApi 1.1.0, Swashbuckle 1.1.2 are unreleased: notes extended, no bump.

## R8 – Documented problem `type` vs runtime

- **Finding**: `ProblemDetailsMapper` turns `about:blank` into null, so the native pipeline fills the
  RFC 9110 link (`ProblemDetailsDefaults`, also used by `ProblemDetailsFactory` and `TypedResults.Problem`).
  The OpenAPI side used `ResultTypes.GetOrDefault` and documented `about:blank`.
- **Decision**: an internal table in Viana.Results.OpenApi mirroring the ASP.NET Core defaults
  (400, 401, 403, 404, 405, 406, 408, 409, 412, 415, 422, 426, 500, 502, 503, 504 → RFC 9110 section
  links). Applied when the resolved type is null/empty/`about:blank`: in `ProblemResultSchema` and when
  serializing problem examples. Statuses outside the table keep `about:blank`. No new dependency.
- **Alternatives**: read the native table (internal in ASP.NET Core, not accessible); keep `about:blank`
  (documentation would differ from the runtime).
