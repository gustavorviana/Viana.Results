# Research: Bodyless Result is 204

## R1 – Runtime change point
- **Decision**: `Results.Ok()` returns `new Result(204)`. Nothing else changes at runtime.
- **Rationale**: `ResultResponseBody.HasBody` already returns false for a bare `Result` and for status 204. The
  AspNetCore writers (`VianaActionResult`, `VianaHttpResult`, both filters) and the Mvc `VianaResultAction`
  write the status without a body. A `new Result(200)` built explicitly keeps 200 (spec edge case).
- **Alternatives**: Map every bodyless 2xx to 204 inside the writers. That was rejected because it would
  silently override explicit statuses such as 202 and would differ from the result's `Status`, which the
  mediator telemetry and user code read.

## R2 – Where the inferred 200 comes from
- **Minimal APIs**: RequestDelegateFactory adds `ProducesResponseTypeMetadata(200, typeof(Result))` for a
  delegate that returns a type which is not an ASP.NET `IResult`. Viana results are such types.
- **Controllers**: ApiExplorer adds a 200 from the return type when the action has no
  `[ProducesResponseType]`/`[Produces]`. A declared 2xx replaces it.
- **Decision**: decide at document time from the endpoint metadata. A success is *declared by hand* when one
  of these is true:
  - (a) an `IProducesResponseTypeMetadata` has a 2xx status and its `Type` is not a Viana result type
    (`Result`, `Result<T>`, `ListResult<T>`, `PagedResult<T>`) or void, or the metadata came from
    `Produces(status)` without a type;
  - (b) a `[ProducesResponseType]` with a 2xx status is on the action or the controller;
  - (c) a `[ResponseExample]` or a `ProducesResult` resolves to a 2xx.

  The success *inferred* from the return type is the 200 whose response type is the action's Viana return
  type, and it is not declared by hand.
- **Caveat**: `Produces<Result>(200)` written explicitly is indistinguishable from the inferred 200 by type.
  This is an accepted limitation, documented in the XML remarks. Use `ProducesResult` or `Produces(200)`
  (no type) instead.

## R2b – Example statuses are hand-declared successes
- `[ResponseExample]` (`StatusCode = 0`) and `ProducesResult<P>()` (`VianaResponseMetadata` with `Status = null`)
  take their status from the provider's result, so the provider must be resolved to know it.
- **Decision**: `ApplyDefaultResult` gets the example success statuses from the existing resolution code:
  `ResponseExampleResolver.Collect` (controllers) and the resolution of `VianaResponseMetadata` used by
  `ResponseExampleOperationTransformer` (minimal APIs). It uses the same `IServiceProvider` (from
  `context.ApplicationServices` / Swashbuckle `context`) and the same global options. The status resolution is
  extracted into a shared internal helper, so the two steps cannot disagree. Providers are created the same way
  as today, by DI or by constructor.
- **Order**: the helper is independent of transformer order. `ApplyDefaultResult` runs first, and the example
  step then adds its responses for the statuses that `ApplyDefaultResult` kept.

## R3 – Single processing step
- **Decision**: replace `ClearBodyForBareResult(operation, method)` with
  `ApplyDefaultResult(operation, method, endpointMetadata)`:
  1. compute the hand-declared success statuses (R2);
  2. remove the inferred 2xx responses that are not declared by hand (with `WithoutDefaultResult`: remove every
     2xx not declared by hand);
  3. for a bare `Result` with no hand-declared success and no `WithoutDefaultResult`, add `204` with
     description "No Content" and no content;
  4. keep clearing the content of bodyless successes for a bare `Result` (today's behavior).

  It must run before the example and problem transformers so they add their responses afterward; the order is
  fixed in `AddVianaResultTransformers` and `AddVianaResultFilters`. The public wrapper in
  `OpenApiOperationProcessor` is renamed (nothing is published).
- **Alternatives**: Remove the metadata at endpoint build time with a convention. That was rejected because
  controllers have no such hook, and it would also change ApiExplorer for other consumers.

## R4 – Marker
- **Decision**: `public sealed class WithoutDefaultResultAttribute : Attribute` with
  `[AttributeUsage(Method | Class, Inherited = true)]`. `.WithoutDefaultResult()` adds an instance with
  `WithMetadata` on `RouteHandlerBuilder`, `RouteGroupBuilder` and `IEndpointConventionBuilder`, matching the
  `ProducesResult` overload set. Detection reads `EndpointMetadata.OfType<WithoutDefaultResultAttribute>()`,
  which contains the attributes of controller actions and controllers too.

## R5 – Interaction with ProducesResult examples
- `ProducesResult<P>()` where P returns `Results.Ok()` now documents 204, because the status comes from the
  result. This is consistent with the runtime.

## R6 – Tests that assert 200 for `Results.Ok()`
- About 25 usages across the core, Mvc, OpenApi and Mediators tests. Each assertion that checks the *status*
  of `Results.Ok()` is updated to 204. Tests that use `Ok()` only as a convenient success keep it.
