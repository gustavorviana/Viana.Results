# Data model: Bodyless Result is 204

No persisted data. The behavior concepts are below.

- **Inferred success**: the 2xx response that a generator derives from the endpoint's Viana return type
  (minimal: RequestDelegateFactory metadata; MVC: ApiExplorer default 200).
- **Hand-declared success**: a 2xx from `Produces`/`[ProducesResponseType]` with a non-Viana or absent type,
  `ProducesResult`, or `[ResponseExample]`.
- **Default result response**: `204 No Content` added for a bare `Result` when no hand-declared success exists.
- **WithoutDefaultResult marker**: endpoint metadata or attribute. When present, no inferred success and no
  default result response are documented.
