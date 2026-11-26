# Endpoint contract

Base routes are the same in the Scalar and Swagger hosts: controllers under `/api/products`, `/api/legacy/products`
and `/api/samples`, and minimal APIs under `/minimal/products`.

| Operation | Route (controller / minimal) | Statuses |
|-----------|------------------------------|----------|
| List | GET `/api/products` · `/minimal/products` | 200 |
| Get | GET `.../{id}` | 200, 404 |
| Create | POST `...` | 201 (Location), 400 (malformed body), validation, 409 |
| Update | PUT `.../{id}` | 204, validation, 404, 409 |
| Delete | DELETE `.../{id}` | 204, 404, business rule |
| Stock | GET `.../{id}/stock` | 200, 404, 503 |
| Inventory toggle | POST `/api/inventory/availability?available=false` | 204 |
| Legacy (Mvc) | GET `/api/legacy/products/{id}` | 200, 404 |
| Samples | GET `/api/samples/{factory}` | one route per core factory and per builder method |
| Crash | GET `/api/samples/crash` | 500 (unhandled exception → ProblemDetails) |

## Mediators example (`/products` + `/scenarios`)
| Route | Statuses / telemetry |
|-------|----------------------|
| CRUD under `/products` (same rules as above) | span `Mediator.Send {Request}`, duration metric |
| GET `/scenarios/throws` | 500; span Error and an exception event (detail level from config) |
| GET `/scenarios/unregistered` | 500 from `HandlerNotFoundException` (RequestType/ResultType shown in detail) |
| GET `/scenarios/unavailable` | 503; span Error (5xx) |
| GET `/scenarios/slow?seconds=10` | 200, or cancellation when the client aborts (span not Error, `viana.mediator.cancelled`) |

Every failure is `application/problem+json`, and its `type` is the native RFC 9110 link.
