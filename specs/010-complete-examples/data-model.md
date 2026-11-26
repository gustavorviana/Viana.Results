# Data model: Complete examples

## Product
| Field | Type | Rules |
|-------|------|-------|
| Id | int | assigned by the catalog, starting at 1 |
| Sku | string | required, 3–20 chars, unique (case-insensitive) → duplicate = Conflict (409) |
| Name | string | required, ≤ 100 chars |
| Price | decimal | > 0 |
| Stock | int | ≥ 0 |

## ProductInput (create/update body)
Sku, Name, Price, Stock. Invalid fields produce one `Results.Validation` problem that lists every field.

## Business rule
Deleting a product with Stock > 0 → `BusinessRuleViolated` problem (shows the factory).

## ProductCatalog
In-memory store, seeded with 3 products at start-up. Create and Update run under one lock, and SKUs
are compared case-insensitively, so concurrent creates with the same SKU yield one success and one Conflict. Operations: List, Get(id), Create,
Update(id), Delete(id), all returning `Result`/`Result<T>`.

## InventoryGateway
Fake external dependency. `Available` is toggled by an endpoint; while it is unavailable, stock queries
return a 503 problem.
