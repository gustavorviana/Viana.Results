# Research: Envelope results

## R1: Where the rule lives

- **Decision**: the rule lives in the core.
  - `ResultReflections.IsEnvelopeType(type)` checks whether the type implements `IEnvelopeResult<>`.
  - `IsUnwrappableType` returns false for envelopes, and the `IPagedResult<>` check is removed because
    `PagedResult<T>` becomes an envelope.
- **Rationale**: Mvc, AspNetCore and OpenApi already call `ResultResponseBody.GetBody` and `IsUnwrappableType`.
  One change reaches all of them, and they cannot drift apart.

## R2: Building the body

- **Decision**: a new overload `ResultResponseBody.GetBody(IResult, JsonSerializerOptions)`.
  - Non-envelopes behave exactly as today.
  - For envelopes, `EnvelopeBody.Build` does the following:
    1. Get `options.GetTypeInfo(type)` and serialize the value to a `JsonObject` through that `JsonTypeInfo`.
    2. Remove the members whose underlying CLR property is `IResult.Status`, `IResult.Problem` or
       `IHasExtensions.Extensions`. The match is made by property name and type against the contract's
       `JsonPropertyInfo`, not by JSON name. This way `[JsonPropertyName]` and naming policies are honored.
    3. If the type implements `IHasExtensions`, add each entry whose key is not already present, compared
       case-insensitively. Each value is serialized with `options.GetTypeInfo(value.GetType())`, and a null value
       is written as JSON null. Declared members win (FR-003).
- The existing `GetBody(IResult)` overload stays. For envelopes it returns the instance itself, which is the
  fallback for non-JSON writers.
- **Rationale**: removing interface members and flattening extensions needs either a converter per type, which
  means generic instantiation and dynamic code, or a node. The node built from `JsonTypeInfo` is AOT-safe when
  the resolver has metadata, and it honors naming policies, converters and `[JsonIgnore]`.
- **Alternatives**:
  - `[JsonExtensionData]`: rejected, because it needs a mutable dictionary property on the user's type.
  - Contract modifiers on the application's options: rejected, because they need a registration (SC-001).

## R3: Writers in each package

- **Decision**:
  - **Mvc**: `VianaResultAction` already serializes with its own options, so it passes them to the new overload.
  - **AspNetCore controllers**: `VianaActionResult` uses the `Microsoft.AspNetCore.Mvc.JsonOptions` serializer
    options. The body is the `JsonObject`, written by the formatters.
  - **AspNetCore minimal APIs**: `VianaHttpResult` uses `Microsoft.AspNetCore.Http.Json.JsonOptions`.
- **Edge**: with Newtonsoft or XML formatters, a `JsonObject` is not their native model. Content negotiation runs
  after the `ObjectResult` is created, so the rule is decided up front:
  - **Controllers**: the `JsonObject` is used when `MvcOptions.OutputFormatters` contains a
    `SystemTextJsonOutputFormatter`. Otherwise the instance itself is used, and `status`/`problem` may appear for
    envelopes that do not derive from `Result` (spec edge case).
  - **Minimal APIs and Mvc**: always System.Text.Json.

## R4: OpenAPI

- **Decision**:
  - `UnwrapResultSchemaTransformer` and `UnwrapResultFilter` already skip non-unwrappable types, so envelopes
    keep their own schema.
  - `SchemaProcessor.OmitProblemFields` (already removes `status`/`problem` from `IResult` schemas) also removes
    `extensions` for `IHasExtensions` envelopes and sets `additionalProperties: true` on them.
  - Examples built from envelope instances use the same `GetBody(result, options)`, so the example equals the
    runtime JSON.
  - Since `PagedResult` already had its own schema, its document is unchanged.

## R5: AOT

- **Decision**: the new core code uses only `JsonSerializerOptions.GetTypeInfo`, `JsonSerializer.SerializeToNode`
  with a `JsonTypeInfo`, and `JsonObject`. None of these carry trimming or dynamic-code annotations.
- **Gap**: the repository has no AOT smoke test (constitution VI requires one), and the core does not set
  `IsAotCompatible`. This feature does not add them. The task list includes a check with the trim/AOT analyzers
  (`IsAotCompatible=true` on a local build of the core) to confirm no new warnings. Adding the CI smoke test is
  left for a separate decision.

## R6: Performance

- **Decision**: the non-envelope path keeps the same calls. Envelopes, `PagedResult` included, pay one
  `SerializeToNode`. A before/after benchmark of writing `PagedResult` and `Result<T>` goes in
  `benchmarks/Viana.Results.Benchmarks`. A regression on `PagedResult` must be recorded and justified (V).
