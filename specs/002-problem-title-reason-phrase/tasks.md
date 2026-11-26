# Tasks: Problem Title Defaults to Status Reason Phrase

**Input**: Design documents from `specs/002-problem-title-reason-phrase/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/default-title.md

**Tests**: Required (constitution Principle III – TDD). Test tasks come first and must fail before
the implementation task that makes them pass.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: Setup

- [X] T001 Run `dotnet build -c Release` and `dotnet test -c Release` from the repository root and record the baseline (0 warnings, all tests passing)

## Phase 2: Foundational

- [X] T002 Add tests in tests/Viana.Results.Tests/HttpStatusInfoTests.cs (create if missing) for an `internal static bool TryGetReasonPhrase(int code, out string phrase)`: 404 → true/"Not Found", 500 → true/"Internal Server Error", 409 → true/"Conflict", 306 and 418 → false (placeholders), 499, 299, 0, -1 → false; also assert `HttpStatusInfo.GetMessage(499)` still returns "Unknown" (confirm `InternalsVisibleTo` for the test assembly exists in src/Viana.Results/Viana.Results.csproj; add it if missing)
- [X] T003 Implement `internal static bool TryGetReasonPhrase(int code, out string phrase)` in src/Viana.Results/HttpStatusInfo.cs reading the existing `httpCodes` table and returning false for 306 and 418; leave public `GetMessage` unchanged; make T002 pass

**Checkpoint**: Reason phrase lookup available; no behavior change yet.

## Phase 3: User Story 1 – Default title for problems created in code (P1) 🎯 MVP

**Goal**: Problems created via constructor, `WithDescription` or `ProblemBuilder` without a title get the reason phrase of their status.

**Independent Test**: `new ProblemResult(404, null!)` and `new ProblemBuilder(404).Build()` (or the builder's existing entry point) have title "Not Found"; explicit titles are unchanged.

### Tests (write first, must fail)

- [X] T004 [P] [US1] In tests/Viana.Results.Tests/ProblemResultTests.cs change the assertion at line 28 from "Error" to the reason phrase of that test's status, and add theories: blank title (null/""/"  ") with 404 → "Not Found", 500 → "Internal Server Error"; `WithDescription(409, "")` → "Conflict"; statuses 306, 418, 499, 0, -1 → "Error"; explicit title "Order already shipped" kept for any status; custom `type` "https://example.com/probs/x" with no title → reason phrase and `type` kept verbatim; blank `type` → "about:blank"
- [X] T005 [P] [US1] In tests/Viana.Results.Tests/ProblemBuilderTests.cs change the assertion at line 11 from "Error" to the reason phrase of that test's status, and add tests: builder with only status 409 → "Conflict"; builder with `WithTitle("Custom")` → "Custom"; builder with unknown status 499 → "Error"

### Implementation

- [X] T006 [US1] In src/Viana.Results/ProblemResult.cs change the private constructor to `Title = string.IsNullOrWhiteSpace(title) ? (HttpStatusInfo.TryGetReasonPhrase(status, out var phrase) ? phrase : "Error") : title;` (keep `"Error"` as a named private const `FallbackTitle`); update the XML doc of the `title` parameters/`Title` property to describe the default; make T004–T005 pass

**Checkpoint**: US1 complete and testable on its own.

## Phase 4: User Story 2 – Same default when reading a document (P2)

**Goal**: Deserialized documents without a title get the same default.

**Independent Test**: Deserializing `{"status":404}` yields title "Not Found".

- [X] T007 [US2] In tests/Viana.Results.Tests/ProblemResultJsonConverterTests.cs update assertions at lines 95, 107, 118 and 143 to the reason phrase of each test's status (500 when status is missing → "Internal Server Error"; keep line 89 input `"Title":"Error"` asserting "Error" as an explicit title); add tests: `{"status":404}` → "Not Found"; `{}` → status 500 and "Internal Server Error"; `{"status":499}` → "Error"; `{"status":404,"title":"Custom"}` → "Custom"; round trip of `new ProblemResult(404, "")` serialize → deserialize keeps "Not Found"
- [X] T008 [US2] In src/Viana.Results/ProblemResultJsonConverter.cs update the comment at line 51 to: missing "title"/"type" are defaulted by the ProblemResult constructor (status reason phrase or "Error" / "about:blank"); no logic change; confirm T007 passes

**Checkpoint**: US1 and US2 both pass.

## Phase 5: Polish & Cross-Cutting

- [X] T009 [P] Run `git grep -n '"Error"' -- src tests` and, if any satellite test under tests/ asserts the old default, update it to the reason phrase
- [X] T010 [P] Update src/Viana.Results/README.md: document that a missing title defaults to the status reason phrase (fallback "Error"), applies to every `type`, explicit `title`/`type` are never changed, and customization belongs to the host (`CustomizeProblemDetails`)
- [X] T011 Bump `<Version>` to 2.1.0 in src/Viana.Results/Viana.Results.csproj and replace `PackageReleaseNotes` with: "Changed: a ProblemResult without a title now defaults to the HTTP reason phrase of its status (e.g. 404 \"Not Found\") instead of \"Error\"; unknown statuses keep \"Error\". Applies to constructors, ProblemBuilder and JSON deserialization. Explicit titles and types are unchanged."
- [X] T012 Run quickstart.md validation: `dotnet build -c Release` (0 warnings, 0 errors) and `dotnet test -c Release` (all pass)

## Dependencies & Execution Order

- T001 → T002 → T003 → (T004 ∥ T005) → T006 → T007 → T008 → (T009 ∥ T010) → T011 → T012
- US2 depends on US1's constructor change (T006), since both entry points share the constructor.

## Parallel Example

```text
US1 tests: T004 (ProblemResultTests.cs) ∥ T005 (ProblemBuilderTests.cs)
Polish:    T009 (satellite tests) ∥ T010 (README)
```

## Implementation Strategy

1. MVP: Phases 1–3 (US1) – covers the most common path.
2. Add US2 (Phase 4) – mostly test updates, since the converter already funnels through the constructor.
3. Polish: docs, version, final validation.
