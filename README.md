# Viana.Results

A .NET library for standardized operation responses using the **Result** pattern,
with first-class support for collections, pagination, a lightweight **Mediator**,
**ASP.NET Core MVC** integration, and **OpenAPI** documentation (native
`Microsoft.AspNetCore.OpenApi` and `Swashbuckle.AspNetCore`).

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![CI](https://github.com/gustavorviana/Viana.Results/actions/workflows/ci.yml/badge.svg)](https://github.com/gustavorviana/Viana.Results/actions/workflows/ci.yml)

---

## Packages

Pick only what you need — every package ships independently.

| Package | NuGet | Downloads | Targets |
|---|---|---|---|
| **Viana.Results** — core types (`Result`, `Result<T>`, `ListResult<T>`, `PagedResult<T>`, `ProblemBuilder`) | [![NuGet](https://img.shields.io/nuget/v/Viana.Results.svg)](https://www.nuget.org/packages/Viana.Results) | [![Downloads](https://img.shields.io/nuget/dt/Viana.Results.svg)](https://www.nuget.org/packages/Viana.Results) | `netstandard2.0`, `net5.0`, `net8.0`, `net10.0` |
| **Viana.Results.Mediators** — minimal Mediator with `Activity` telemetry | [![NuGet](https://img.shields.io/nuget/v/Viana.Results.Mediators.svg)](https://www.nuget.org/packages/Viana.Results.Mediators) | [![Downloads](https://img.shields.io/nuget/dt/Viana.Results.Mediators.svg)](https://www.nuget.org/packages/Viana.Results.Mediators) | `netstandard2.0`, `net5.0`, `net6.0`, `net8.0`, `net10.0` |
| **Viana.Results.Mvc** — `Result → ActionResult` + global `IResult` filter | [![NuGet](https://img.shields.io/nuget/v/Viana.Results.Mvc.svg)](https://www.nuget.org/packages/Viana.Results.Mvc) | [![Downloads](https://img.shields.io/nuget/dt/Viana.Results.Mvc.svg)](https://www.nuget.org/packages/Viana.Results.Mvc) | `net6.0`, `net8.0`, `net10.0` |
| **Viana.Results.OpenApi** — native `Microsoft.AspNetCore.OpenApi` transformers (no Swashbuckle) | [![NuGet](https://img.shields.io/nuget/v/Viana.Results.OpenApi.svg)](https://www.nuget.org/packages/Viana.Results.OpenApi) | [![Downloads](https://img.shields.io/nuget/dt/Viana.Results.OpenApi.svg)](https://www.nuget.org/packages/Viana.Results.OpenApi) | `net10.0` |
| **Viana.Results.OpenApi.Swashbuckle** — `SwaggerGen` filters for the same OpenAPI output | [![NuGet](https://img.shields.io/nuget/v/Viana.Results.OpenApi.Swashbuckle.svg)](https://www.nuget.org/packages/Viana.Results.OpenApi.Swashbuckle) | [![Downloads](https://img.shields.io/nuget/dt/Viana.Results.OpenApi.Swashbuckle.svg)](https://www.nuget.org/packages/Viana.Results.OpenApi.Swashbuckle) | `net10.0` |

Both OpenAPI packages produce an identical OpenAPI document — pick the one
that matches the pipeline you already use.

---

## Install

```bash
# Core (required by every other package)
dotnet add package Viana.Results

# Optional, mix and match:
dotnet add package Viana.Results.Mediators
dotnet add package Viana.Results.Mvc
dotnet add package Viana.Results.OpenApi              # native AddOpenApi pipeline
dotnet add package Viana.Results.OpenApi.Swashbuckle  # SwaggerGen pipeline
```

---

## Quick start

```csharp
using Viana.Results;

public Result<User> GetUser(int id)
{
    var user = repository.Find(id);
    if (user is null)
        return Results.NotFound("User not found"); // 404 + ProblemDetails

    return user; // implicit conversion → Result<User>
}
```

In an ASP.NET Core controller, with `Viana.Results.Mvc`:

```csharp
using Viana.Results.Mvc;

[HttpGet("{id}")]
public Task<IActionResult> Get(int id) =>
    _service.GetUserAsync(id).ContinueWith(t => t.Result.ToActionResult());
```

Or register the global filter once and return `IResult` directly from actions:

```csharp
services.AddControllers().AddVianaResultFilter();
```

```csharp
[HttpGet("{id}")]
public async Task<IResult> Get(int id) => await _service.GetUserAsync(id);
```

---

## Documentation

Each package has its own README with full API surface, registration, and
examples:

- [Viana.Results](src/Viana.Results/README.md) — core result types, `Results` factory, `ProblemBuilder`
- [Viana.Results.Mediators](src/Viana.Results.Mediators/README.md) — `IRequest<T>`, `IHandler<,>`, DI registration, Activity tags
- [Viana.Results.Mvc](src/Viana.Results.Mvc/README.md) — `ToActionResult()`, `VianaResultFilter`
- [Viana.Results.OpenApi](src/Viana.Results.OpenApi/README.md) — native OpenAPI transformers, `[ProblemResult]`, `[ResponseExample]`
- [Viana.Results.OpenApi.Swashbuckle](src/Viana.Results.OpenApi.Swashbuckle/README.md) — same surface for `SwaggerGen`

---

## Contributing

Issues and pull requests are welcome at
[`gustavorviana/Viana.Results`](https://github.com/gustavorviana/Viana.Results).

## License

[MIT](LICENSE)
