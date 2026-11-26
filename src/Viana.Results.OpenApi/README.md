## Viana.Results.OpenApi

Makes the native `AddOpenApi()` document match what Viana results really send. `Result<T>` is documented as
`T`, and failures as RFC 9457 problems. For `SwaggerGen`, use `Viana.Results.OpenApi.Swashbuckle`.

```bash
dotnet add package Viana.Results.OpenApi
```

Target: `net10.0`.

### Register

```csharp
builder.Services.AddOpenApi(o => o.AddVianaResultTransformers());

app.MapOpenApi();
```

### Document errors and examples (controllers)

```csharp
[HttpGet("{id}")]
[ProblemResult(404, Title = "User not found")]
[ResponseExample(typeof(UserExample))]
public Result<User> Get(int id) => _service.GetUser(id);

public sealed class UserExample : IExampleProvider<Result<User>>
{
    public Result<User> GetExample() => new User { Id = 42, Name = "Ana" };
}
```

### Minimal APIs

```csharp
app.MapGet("/users/{id}", handler)
   .ProducesResult<UserExample>()
   .ProducesProblemResult(404, title: "User not found");
```

A bare `Result` is documented as 204. Endpoints that require authorization get 401/403 automatically.

Examples: [examples](https://github.com/gustavorviana/Viana.Results/tree/main/examples).
