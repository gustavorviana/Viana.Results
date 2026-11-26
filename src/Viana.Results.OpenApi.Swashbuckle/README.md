## Viana.Results.OpenApi.Swashbuckle

Makes the `SwaggerGen` document match what Viana results really send. It produces the same document as
`Viana.Results.OpenApi`.

```bash
dotnet add package Viana.Results.OpenApi.Swashbuckle
```

Target: `net10.0`. Requires Swashbuckle 10.x.

### Register

```csharp
builder.Services.AddSwaggerGen(o => o.AddVianaResultFilters());
```

### Document errors and examples

```csharp
[HttpGet("{id}")]
[ProblemResult(404, Title = "User not found")]
[ResponseExample(typeof(UserExample))]
public Result<User> Get(int id) => _service.GetUser(id);

app.MapGet("/users/{id}", handler)
   .ProducesResult<UserExample>()
   .ProducesProblemResult(404, title: "User not found");
```

Attributes, extensions and example providers are the same as in `Viana.Results.OpenApi`.

Examples: [examples](https://github.com/gustavorviana/Viana.Results/tree/main/examples).
