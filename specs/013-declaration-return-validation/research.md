# Research: Validate response declarations against the return type

## R1: What counts as a result return

- **Decision**: a type is a result return when either of these holds:
  - it implements the Viana `IResult`;
  - it is `Task<T>`, `ValueTask<T>` or `ActionResult<T>` whose `T` is a result return, checked recursively.

  Examples of result returns are `Task<Result<T>>`, `ValueTask<Result>`, `Task<IResult>` (Viana),
  `ActionResult<Result<T>>` and `Task<ActionResult<Result<User>>>`. Non-generic `IActionResult`, `ActionResult`,
  `object`, the ASP.NET Core `IResult`, `void`, `Task`, `Task<UserDto>`, collections (`List<Result>`,
  `IEnumerable<Result<T>>`) and `Dictionary<string, Result>` are not.
- **Rationale**: the user's rule. Only the wrappers used to declare an action's return count.
- **Alternatives**: any generic, or any single-argument generic. Both were rejected because they accept
  collections and dictionaries.
- **Consequence**: `ApplyDefaultResult` and `ApplyProblemResultAttributes` today unwrap only tasks. They will
  use the same predicate, and they need the inner result type for the bare-`Result`/204 rule. A
  `ResultTypeOf(Type)` returns the first result type found, depth-first.

## R2: Telling controller actions from minimal APIs

- **Decision**: an operation is a controller action when its action descriptor is a `ControllerActionDescriptor`.
  Its controller type comes from `ControllerTypeInfo`, not from `method.DeclaringType`, so that an action
  inherited from a base controller counts for the derived controller. Every other operation is a minimal API.
  For minimal APIs, class-level attributes are never read, because the delegate's declaring type is a compiler
  closure.
- **Note**: `ExampleResolver.Collect` reads class attributes from `method.DeclaringType` today. That misses
  attributes on a derived controller when the action is inherited. It will receive the controller type.

## R3: Where each declaration comes from

- **Decision**:
  - Method-level means `method.GetCustomAttributes(inherit: true)` for `[ProblemResult]`, `[ResponseExample]` and
    `[WithoutDefaultResult]`.
  - Class-level means the same attributes on the controller type, with inherit enabled.
  - For minimal APIs, it means `VianaResponseMetadata` and `WithoutDefaultResultAttribute` in the endpoint
    metadata.

  If the same attribute appears at both levels, the method-level copy decides: error when the action is not a
  result.

## R4: "No result action in the controller"

- **Decision**: a controller's actions are its public instance methods, including inherited ones, excluding:
  - `[NonAction]` methods;
  - special names (property accessors);
  - methods declared on `object`, `ControllerBase` and `Controller`.

  The check is cached in a `ConcurrentDictionary<Type, bool>`. It runs when the first operation of that
  controller is processed, so the error appears even when every action is non-result. Each action reports it.
  The first one throws, and generation stops.
- **Alternative**: a document-level transformer or filter. It was rejected because it adds a second registration
  per pipeline for one boolean.

## R5: Warning channel

- **Decision**: `ILogger` with the category `Viana.Results.OpenApi`, taken from `ILoggerFactory` in DI. When no
  logger factory is registered, nothing is logged. Message:
  `"{Controller}.{Action}: class-level [{Attribute}] skipped, the action does not return a Viana result."`
- **Once per generation**: transformers and filters run once per operation per document, so one warning per
  action and attribute per generation follows naturally.

## R6: Error type and message

- **Decision**: `InvalidOperationException`, consistent with 012. The messages are:
  - Minimal API: `"Endpoint '{METHOD} {route}': {declaration} requires the endpoint to return a Viana result, but it returns {type}."`
  - Action: `"Action '{Controller}.{Action}': [{Attribute}] requires the action to return a Viana result, but it returns {type}."`
  - Controller: `"Controller '{Controller}': [{Attribute}] is declared on the class but no action returns a Viana result."`

## R7: Global examples

- **Decision**: global examples are only collected for result endpoints. They never cause errors or warnings.
