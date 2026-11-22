# Contract: Problem Response

1. Content type is `application/problem+json` (FR-005).
2. Members: `type`, `title`, `status`, optional `detail` and `instance`, `traceId`, Viana extensions.
3. Parity: for status in {400, 401, 403, 404, 409, 422, 500} and equal title/type/detail, the body
   equals the body of `ControllerBase.Problem(...)` in the same app, ignoring the `traceId` value
   (FR-004, SC-001).
4. Precedence: explicit Viana values are unchanged unless `CustomizeProblemDetails` changes them; a
   Viana `traceId` extension wins over the native one (FR-003, SC-002); customization is invoked exactly
   once per response (SC-003).
5. Works with and without `services.AddProblemDetails()`.
