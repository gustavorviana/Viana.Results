# Changelog

Versions are managed by release-please from Conventional Commits. The entries below were written
before release-please was adopted (feature 017) and are not yet released.

## Unreleased (since Viana.Results.AspNetCore 1.0.0)

- Initial release: MVC integration with native ProblemDetails interop.
- Minimal APIs: VianaHttpResult, ToHttpResult() (and the Task overload) and AddVianaResults() (endpoint filter for endpoints and groups), with the same rules as MVC.
- Results.Ok() without a value now answers 204 with no body (via Viana.Results); bodyless results keep their own status.
- Added: envelope results (IEnvelopeResult<T>) are written whole by controllers and minimal APIs with the application JSON options; with only non-System.Text.Json formatters (Newtonsoft, XML) the instance itself is written.
