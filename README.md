# Viana.Results

Result pattern for .NET: return `Result`/`Result<T>` from your code and get the right HTTP status, RFC 9457
problem details and an accurate OpenAPI document.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![CI](https://github.com/gustavorviana/Viana.Results/actions/workflows/ci.yml/badge.svg)](https://github.com/gustavorviana/Viana.Results/actions/workflows/ci.yml)

## Packages

| Package | Use it for | Targets |
|---|---|---|
| [Viana.Results](src/Viana.Results/README.md) | Core types (`Result`, `Result<T>`, `ListResult<T>`, `PagedResult<T>`) | `netstandard2.0`, `net8.0`, `net10.0` |
| [Viana.Results.AspNetCore](src/Viana.Results.AspNetCore/README.md) | Controllers and minimal APIs with native ProblemDetails | `net8.0`, `net10.0` |
| [Viana.Results.Mvc](src/Viana.Results.Mvc/README.md) | MVC on net6.0 / existing apps | `net6.0`, `net8.0`, `net10.0` |
| [Viana.Results.Mediators](src/Viana.Results.Mediators/README.md) | Small mediator with traces and metrics | `netstandard2.0`, `net6.0`, `net8.0`, `net10.0` |
| [Viana.Results.OpenApi](src/Viana.Results.OpenApi/README.md) | OpenAPI with `AddOpenApi()` (Scalar, etc.) | `net10.0` |
| [Viana.Results.OpenApi.Swashbuckle](src/Viana.Results.OpenApi.Swashbuckle/README.md) | OpenAPI with `SwaggerGen` | `net10.0` |

## Quick start

```bash
dotnet add package Viana.Results
dotnet add package Viana.Results.AspNetCore
```

```csharp
public Result<User> GetUser(int id)
{
    var user = repository.Find(id);
    if (user is null)
        return Results.NotFound("User not found"); // 404 + problem details

    return user; // 200 with the user
}
```

```csharp
builder.Services.AddControllers().AddVianaResultFilter();

[HttpGet("{id}")]
public Result<User> Get(int id) => _service.GetUser(id);
```

Runnable examples: [examples](examples/README.md).

## Contributing

Issues and pull requests are welcome. The build treats warnings as errors
([Directory.Build.props](Directory.Build.props)), and hot-path changes need before/after
[benchmarks](benchmarks/README.md).

## License

[MIT](LICENSE)
