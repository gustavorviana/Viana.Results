# Tasks: Validate response declarations against the return type

**Input**: [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [data-model.md](data-model.md),
[contracts/validation-rules.md](contracts/validation-rules.md), [quickstart.md](quickstart.md)

**Tests**: required. The constitution (III) mandates test-first. Each story's tests are written first and must
fail. Tests use one type per file under `tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/`.

## Phase 1: Setup

- [X] T001 Audit the declarations already in the repository for cases that would start failing or warning:
      `[ProblemResult]`, `[ResponseExample]` and `[WithoutDefaultResult]` on non-result actions or classes, and
      `ProducesResult`, `ProducesProblemResult` and `WithoutDefaultResult` on non-result minimal APIs. Check
      `tests/` and `examples/`. List the findings in this file under "Audit". A case that is a deliberate test of
      the old silent behavior becomes a test of the new rule. Fix the other cases by returning a result.

## Phase 2: Foundational (blocks all stories)

- [X] T002 [P] Write failing unit tests for the result-return predicate in
      `tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/ResultReturnTests.cs`, following R1: "Viana
      `IResult`, or `Task<T>`/`ValueTask<T>`/`ActionResult<T>` whose `T` is a result return, recursively".
      - Accepted: `Result`, `Result<T>`, `ListResult<T>`, the Viana `IResult`, `Task<Result<T>>`, `ValueTask<Result>`,
        `ActionResult<Result<T>>`, `Task<ActionResult<Result<User>>>`.
      - Refused: `IActionResult`, `ActionResult`, `object`, the ASP.NET Core `IResult`, `void`, `Task`,
        `Task<UserDto>`, `List<Result>`, `IEnumerable<Result<T>>`, `Task<List<Result>>`,
        `Dictionary<string, Result>`.
      - Also accepted: `Task<IResult>` (Viana), `IResult<User>`, `IListResult<User>`, `IPagedResult<User>`,
        `Task<ActionResult<IResult<User>>>`.
      - Also test `ResultTypeOf` on the same types: it returns the first result type found, depth-first, or null.
- [X] T003 Create the internal `ReturnTypeRules` in `src/Viana.Results.OpenApi/Processing/ReturnTypeRules.cs` with:
      - `IsResultReturn(Type)` and `ResultTypeOf(Type)` (R1);
      - `ControllerHasResultAction(Type controller)` (R4): public instance methods, including inherited ones,
        excluding `[NonAction]` methods, special names, and methods declared on `object`, `ControllerBase` or
        `Controller`, with the answer cached in `ConcurrentDictionary<Type, bool>`;
      - the three message builders from R6.

      Make T002 pass.
- [X] T004 [P] Write failing tests in `tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/ActionResultWrappedTests.cs`,
      with the controller in `ActionResultWrappedController.cs`. An `ActionResult<Result>` action documents 204,
      and an `ActionResult<Result<T>>` action with `[ProblemResult(404)]` documents 404. Run them and confirm
      they fail.
- [X] T004b Replace `UnwrapTaskType` + `typeof(IResult).IsAssignableFrom` in `ApplyDefaultResult` and
      `ApplyProblemResultAttributes` (`src/Viana.Results.OpenApi/Processing/OperationProcessor.cs`) with
      `ResultTypeOf`. The bare-`Result` (204) rule uses the found type. Make T004 pass.
- [X] T005 Give every transformer in `src/Viana.Results.OpenApi/Transformers/` and every filter in
      `src/Viana.Results.OpenApi.Swashbuckle/Filters/` what the rules need:
      - the controller type, taken from `ControllerActionDescriptor.ControllerTypeInfo`, or null for minimal
        APIs (R2);
      - an optional `ILogger` with the category `Viana.Results.OpenApi`, from `ILoggerFactory` in DI (R5).

      Pass both into the `OperationProcessor.Apply*` methods. `ExampleResolver.Collect`
      (`src/Viana.Results.OpenApi/ExampleResolver.cs`) reads class attributes from the controller type instead
      of `method.DeclaringType`. For minimal APIs it reads none.

**Checkpoint**: the existing suite stays green (FR-006).

## Phase 3: User Story 1, minimal API non-result fails (P1) 🎯 MVP

**Independent test**: a `string` handler with `ProducesResult<User>()` fails, naming the endpoint. The same
declaration on a `Task<Result<User>>` handler passes.

- [X] T006 [P] [US1] Write failing tests in `tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/MinimalApiValidationTests.cs`.
      Each of `ProducesResult<T>()`, `ProducesResult<P>()` (provider), `ProducesProblemResult(...)`,
      `ProducesProblemResult<P>()` and `WithoutDefaultResult()` on a handler returning `string` throws
      `InvalidOperationException`. The message contains the HTTP method, the route, the declaration name and
      `String`. Handlers returning `Task<Result<User>>` or `Result` do not throw. Global examples on a `string`
      handler neither throw nor apply (R7).
- [X] T007 [US1] In `OperationProcessor.ApplyEndpointDeclarations` and `ApplyEndpointDeclarationsAsync`, plus
      `ApplyDefaultResult` for `WithoutDefaultResultAttribute` in the metadata, throw the R6 minimal-API message
      when Viana metadata is present and `IsResultReturn` is false. The method and its return type come from the
      endpoint's `MethodInfo`. Make T006 pass.
- [X] T008 [US1] Add an end-to-end host test that generates the document on both pipelines (native and
      Swashbuckle) and asserts the same failure (FR-007), in
      `tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/MinimalApiPipelineTests.cs`. Put marker types in
      their own files.

## Phase 4: User Story 2, action attribute on non-result fails (P1)

**Independent test**: `[ProblemResult(404)]` on an action returning `UserDto` fails. On
`ActionResult<Result<UserDto>>` it passes.

- [X] T009 [P] [US2] Write failing tests in `tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/ActionAttributeValidationTests.cs`,
      with the controller in `NonResultActionController.cs`. Each of `[ProblemResult]`, `[ResponseExample]`
      (problem and success) and `[WithoutDefaultResult]` placed directly on an action returning `UserDto`,
      `IActionResult` or `Task` throws. The message contains the controller, the action, the attribute and the
      type. If the same attribute is on both the method and the class, the method decides (R3).
- [X] T010 [US2] In `OperationProcessor` (`ApplyProblemResultAttributes`, `ApplyResponseExamples` and
      `ApplyDefaultResult`), when the action is a controller action and a method-level attribute is present
      while `IsResultReturn` is false, throw the R6 action message. This replaces the current silent
      `return`s. Make T009 pass.
- [X] T011 [US2] Add pipeline parity tests for one action case on both pipelines, in
      `tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/ActionAttributePipelineTests.cs`.

## Phase 5: User Story 3, class attribute (P2)

**Independent test**: class-level `[ProblemResult(500)]` with a result action and a DTO action: 500 appears only
on the result action, and one warning is logged. A class with no result action fails.

- [X] T012 [P] [US3] Write failing tests in `tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/ClassAttributeValidationTests.cs`.
      Each controller goes in its own file: `MixedController.cs`, `NoResultController.cs`, `BaseResultController.cs`
      and `DerivedController.cs`, plus a `ListLogger.cs` that captures log entries.
      - Mixed: only the result action gets the declaration, and exactly one Warning per skipped action and
        attribute is logged. It names the controller, the action and the attribute.
      - No result action: throws the R6 controller message.
      - Inherited class attribute and inherited action: the derived controller is evaluated (R2).
      - `[NonAction]` methods and properties are ignored when looking for a result action.
      - A controller whose only result action is declared as the Viana `IResult` counts as having a result action.
      - Generating two documents logs the warning once per document.
      - No logger registered: no exception, and the declarations are still skipped.
- [X] T013 [US3] Implement the class-level rules in `OperationProcessor`, using `ControllerHasResultAction`
      first (error), then skipping with `ILogger.LogWarning` (R5 message) for non-result actions. Cover
      `[ProblemResult]`, `[ResponseExample]` and `[WithoutDefaultResult]`. Make T012 pass.
- [X] T014 [US3] Add pipeline parity tests for the mixed and no-result controllers on both pipelines, in
      `tests/Viana.Results.OpenApi.Tests/ReturnTypeValidation/ClassAttributePipelineTests.cs`.

## Phase 6: Polish

- [X] T015 [P] Keep the READMEs minimal: at most one line in `src/Viana.Results.OpenApi/README.md` and
      `src/Viana.Results.OpenApi.Swashbuckle/README.md` saying that declarations require a Viana result return.
      Record the full rules (what counts as a result return, the errors, the class-level warning) for the wiki
      task. Do not name any other mediator library.
- [X] T016 [P] Add release notes in `src/Viana.Results.OpenApi/Viana.Results.OpenApi.csproj` and
      `src/Viana.Results.OpenApi.Swashbuckle/Viana.Results.OpenApi.Swashbuckle.csproj`, escaping `<`/`>`, with
      no version bump.
- [X] T017 Run [quickstart.md](quickstart.md): Release build with 0 warnings and the full test suite on every
      TFM. Open the three examples and check that no error and no `Viana.Results.OpenApi` warning appears. Set
      the spec status to Ready.

## Audit

Two cases in the repository started failing under the new rules, and both were fixed by returning a result:

- `tests/Viana.Results.OpenApi.Tests/Swashbuckle/SwaggerPathsTests.cs`: a minimal API returned the ASP.NET Core
  `IResult` with `ProducesProblemResult`. It now returns `Results.Ok()` (Viana).
- `examples/Viana.Results.Examples.Shared/Controllers/LegacyProductsController.cs`: an `IActionResult` action had
  `[ResponseExample]`. It is now declared as `ActionResult<Result<Product>>`, and still returns `VianaResultAction`.

No class-level or group-level Viana declarations exist in the examples, so they produce no warning.

Implementation note: T007, T010 and T013 were applied in one pass over `OperationProcessor.cs`, after all
three stories' tests were written and failing, because they share the same `ReturnTypeRules.Applies` gate.

## Dependencies

- T004 (red) → T004b (green).
- T001 → T002–T005 → US1, US2 and US3 (independent of each other once Phase 2 is done) → Polish.
- Within each story: tests → implementation → pipeline parity.
- T007, T010 and T013 all touch `OperationProcessor.cs`, so they run sequentially.

## Parallel examples

- Phase 2: T002 runs in parallel with the reading part of T001.
- The stories' test tasks T006, T009 and T012 are in separate files and can be written together.
- Polish: T015 ∥ T016.

## Implementation strategy

The MVP is Phase 2 plus US1. Then US2, the other P1, then US3. Commit separately for docs, examples and code.
