# Research: Bug-fix Sweep — Results, Mvc and OpenApi

## R1 – I1 null body
- **Decision**: In `ResultResponseBody.GetBody`, return `resultData.Data` (may be null) for unwrappable
  non-scalar results; remove the `?? new object()` fallback. Callers that need "no body" use 204.
- **Note**: a null body means the formatter writes `null` (JSON literal). Mvc writes `null`.

## R2 – MV3 problem first
- **Decision**: Check `result.Problem != null` before the `Status == 204` short-circuit.

## R3 – I8 simple types
- **Decision**: `IsScalarLike` returns true only for: primitives (`bool`, `char`, integral and floating
  types), `decimal`, `string`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, `Uri`, `Version`,
  enums, `DateOnly`/`TimeOnly` (net6.0+, checked by name to stay netstandard2.0-safe), and `Nullable<T>`
  of those. Other structs are unwrapped like classes. OpenAPI uses the same helper, so docs follow.

## R4 – I9 immutable empty extensions
- **Decision**: Return a shared static empty `ReadOnlyDictionary<string, object?>` when there are no members.

## R5 – MV1 / MV2 Mvc writing
- **Decision**: Content type `application/problem+json` when `result.Problem != null`, otherwise the
  current `ContentType` (`application/json`). Write with
  `await JsonSerializer.SerializeAsync(response.Body, body, body.GetType(), options, context.HttpContext.RequestAborted)`.
  The protected virtual `SerializeToJson` stays (public extension point, no breaking change): the
  streaming path is used unless a subclass overrides `SerializeToJson`, detected once per type via
  reflection (`GetMethod(...).DeclaringType != typeof(VianaResultAction)`), in which case the override
  keeps being honored.
- **Abort**: catch `OperationCanceledException` when `RequestAborted` is cancelled and return.

- **Benchmark (MvcBenchmarks, net10.0, i7-11390H)**:
  | Method | Before Mean | Before Allocated | After Mean | After Allocated |
  |--------|------------:|-----------------:|-----------:|----------------:|
  | Success | 1.683 us | 2.99 KB | 1.599 us | 2.67 KB |
  | Failure | 1.364 us | 2.89 KB | 1.312 us | 2.55 KB |
- **I1 in Mvc**: a successful typed result whose value is null is written as the JSON literal `null`
  (new internal `ResultResponseBody.HasBody` tells "no body" apart from "null body").

## R6 – MV4 idempotent registration
- **Decision**: `TryAddScoped<VianaResultFilter>` and `TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, VianaResultFilterSetup>())`
  with a dedicated `VianaResultFilterSetup : IConfigureOptions<MvcOptions>` class (TryAddEnumerable needs a concrete implementation type).

## R7 – B1 example node
- **Decision**: Build the example with `JsonObject` + `JsonValue.Create(value)` / `JsonSerializer.SerializeToNode(value, options)`
  per field; `ProblemFieldSchema.GetExampleJsonField/Value` become obsolete (M1) and unused internally.

## R8 – B2 default title
- **Decision**: Default title in `ProblemResultSchema` = `HttpStatusInfo.TryGetReasonPhrase` (core
  internal, visible to OpenApi) or "Error"; same rule as runtime (feature 002). Response `Description`
  uses the same title.

## R9 – B4 ValueTask
- **Decision**: Shared internal `UnwrapTaskType(Type)` handling `Task<>` and `ValueTask<>` (by generic
  type definition, not by name); used by `ClearBodyForBareResult` and `ApplyProblemResultAttributes`.

## R10 – B5 authorization
- **Decision**: Add internal overloads taking `IEnumerable<object> endpointMetadata`; transformers and
  Swashbuckle filters pass `ApiDescription.ActionDescriptor.EndpointMetadata`. Rule: authorized when
  metadata contains `IAuthorizeData` and does not contain `IAllowAnonymous` (same interfaces ASP.NET
  Core uses; covers `[Authorize]`, `[AllowAnonymous]`, `RequireAuthorization()`, `AllowAnonymous()`).
  Public method-only overload kept (obsolete per M1).

## R11 – M1 obsolete helpers
- **Decision**: `[Obsolete("Internal helper; it will become internal in Viana.Results.OpenApi 2.0.0.")]` on:
  `OpenApiOperationProcessor`, `OpenApiDocumentProcessor`, `OpenApiSchemaProcessor`,
  `ResponseExampleResolver`, `ResolvedExample`, `ProblemFieldSchema`. `ProblemResultSchema` stays public
  but its low-level members (`AddField` overloads, `Build`, `ApplyTo`) are marked; library code calls
  internal equivalents. Add `InternalsVisibleTo("Viana.Results.OpenApi.Swashbuckle")` and tests to OpenApi.

## R12 – M2 options
- **Decision**: `AddVianaResultExamples` uses `services.Configure<VianaResultOptions>(configure)`;
  consumers read `IOptions<VianaResultOptions>`. Keep resolving a directly registered
  `VianaResultOptions` singleton for backward compatibility (merge both).

## R13 – M3 provider creation
- **Decision**: `ActivatorUtilities.CreateInstance(services, providerType)` when services exist;
  wrap failures in `InvalidOperationException($"Could not create example provider '{type.FullName}': {inner.Message}", inner)`.

## R14 – M5 / I10 case-insensitive member names
- **Decision** (owner, 2026-11-22): extension member names are case-insensitive across the library.
  Core: `ProblemResult.NormalizeExtensions`, `ProblemBuilder` and `ProblemResultJsonConverter.Read` use
  `StringComparer.OrdinalIgnoreCase`; assignment by indexer gives "last value wins, first casing kept".
  OpenAPI: `ProblemResultSchema` uses the same comparer and rule (update the existing field instead of
  `TryAdd` dropping it); no `PropertyNamingPolicy` (the runtime writes keys as stored); no warning or
  exception.
- **Rationale**: documentation equals runtime; no breaking failure at document generation (D1 resolved).
- **Alternatives rejected**: throw at generation (breaking, D1); warn and keep first (doc ≠ runtime);
  case-sensitive schema (owner chose case-insensitive).

## R15 – M6 Swashbuckle unwrap
- **Decision**: Replace only `media.Schema` on each existing `OpenApiMediaType` (keep `Example`,
  `Examples`, `Encoding`, `Extensions`).

## R17 – M2 backward compatibility
- `AddVianaResultExamples` uses `services.Configure(configure)` and also registers
  `VianaResultOptions` as a singleton resolved from `IOptions<VianaResultOptions>.Value`, so code that
  injects `VianaResultOptions` directly keeps working.

## R16 – Versions
- Core 3.0.0 (unreleased), Mvc 2.0.0 → 2.1.0, OpenApi 1.1.0 and Swashbuckle 1.1.2 (unreleased; notes extended).
