# Quickstart: validate feature 014

## Automated

```bash
dotnet build -c Release              # 0 warnings
dotnet test -c Release               # every TFM
```

The envelope tests check the following scenarios:

1. A `CursorResult<User>` written by Mvc, AspNetCore controllers and AspNetCore minimal APIs gives
   `{ "data": [...], "nextCursor": "abc" }`, with no `status` or `problem`.
2. `[JsonPropertyName("items")]` gives `items` at runtime and in the schema.
3. An envelope with extensions puts the entries at the root, with no `extensions` member. A declared property
   wins on a name clash.
4. A problem on an envelope gives `application/problem+json`, and status 204 gives no body.
5. The existing `PagedResult` tests pass unchanged.
6. On the native pipeline and on Swashbuckle, the schema members of a `CursorResult<User>` endpoint equal the
   runtime body members, and `additionalProperties` is allowed when the type has extensions.

## Benchmarks

```bash
dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter *Envelope* *Paged*
```

Compare the results with the baseline and record them in the PR.

## AOT check (local)

Build `src/Viana.Results` with `-p:IsAotCompatible=true`. There must be no new trim or AOT warnings.

## Manual

Run the Scalar example and call the cursor endpoint. Its body and its documented schema must match.
