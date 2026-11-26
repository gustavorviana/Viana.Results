# Research: Review fixes

## R1: Contract-based shaping decision

- **Finding** (from 014): `JsonTypeInfo.Properties` still lists `Status`/`Problem` when they carry `[JsonIgnore]`.
  For this reason, 014 decided from the first serialized instance, and the review flagged that: a conditional rule
  hides the member only for some instances.
- **Decision**: a container member (`Status`, `Problem`, or `Extensions` for `IHasExtensions` types) counts as
  *never written* only when one of these holds:
  - its `JsonPropertyInfo.Get` is null; or
  - its CLR property has `[JsonIgnore]` with `Condition = Always`, the default, read through
    `JsonPropertyInfo.AttributeProvider`.

  Any other case means the member *may be written*: a conditional ignore (`WhenWritingDefault`,
  `WhenWritingNull`), a `ShouldSerialize` delegate, or a plain property. A type with at least one member that may be
  written, or any `IHasExtensions` type, is always shaped. The decision is cached per `JsonTypeInfo`, as today.
- **Safe side on purpose**: members hidden only by global options (`DefaultIgnoreCondition`,
  `IgnoreReadOnlyProperties`, contract modifiers) count as "may be written". Those envelopes are shaped, which is
  correct and only slightly more expensive. When in doubt, shape.
- **Effect**: `PagedResult`, which uses plain `[JsonIgnore]`, is still written as the instance, so the allocation
  is unchanged. Envelopes with conditional rules are always shaped and never leak the members.
- **Alternatives**: always shape envelopes. Rejected, because it brings back the 014 regression on `PagedResult`.

## R2: Authorization responses

- **Decision**: `operation.Responses ??= new OpenApiResponses();` runs before the 401/403 schemas are applied, and
  only when authorization is required. This matches the sibling steps.

## R3: Problem examples on empty operations

- **Decision**: the early return on null/empty `Responses` is replaced by `operation.Responses ??= new OpenApiResponses();`.
  The non-problem path already skips statuses without an existing response (`TryGetValue`), so success examples
  are still ignored there, as US2 scenario 3 requires.

## R4: Version announcements

- **Decision**: during implementation, search `src/`, `tests/`, `examples/`, `README.md`, the package READMEs and
  the csproj release notes for version-dated removal or breaking-change wording, such as "removed in X.Y.Z",
  "will be removed", "breaking change" or "until … X.Y.Z". Then rewrite each case:
  - obsolete messages and XML docs name the replacement only;
  - release notes describe the change with no planned version;
  - test comments drop the version.
- `specs/` is excluded, because it is the historical record.
