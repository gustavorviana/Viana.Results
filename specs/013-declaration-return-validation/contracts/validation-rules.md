# Contract: declaration validation

There is no new public API. The contract covers behavior and messages.

## Errors (`InvalidOperationException`, thrown during document generation)

| Case | Message must contain |
|---|---|
| Minimal API, non-result | the endpoint (HTTP method and route), the declaration, the returned type |
| Action attribute, non-result | the controller, the action, the attribute name, the returned type |
| Class attribute, no result action | the controller, the attribute name |

## Warning (`ILogger`, category `Viana.Results.OpenApi`, level Warning)

The warning is logged for a class attribute on a non-result action. It must contain the controller, the action
and the attribute name, and it is logged once per action and attribute per generated document.

## Unchanged

Documents for endpoints already recognized as results (directly or through `Task`/`ValueTask`) are
byte-identical to the current output.

## Changed

`ActionResult<T>` wrappers that carry a result, such as `ActionResult<Result<T>>`, now receive the
declarations.

## Pipelines

The native transformers and the Swashbuckle filters produce the same outcomes.
