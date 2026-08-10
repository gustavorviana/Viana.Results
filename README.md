# Viana.Results

A **Result** pattern for .NET that turns your domain outcomes into correct HTTP
responses — with **RFC 9457 Problem Details** on errors and **OpenAPI/Swagger docs
that write themselves**.

The core is `netstandard2.0` with **zero ASP.NET dependency**: your domain and
application layers return `Result<T>` without dragging the web framework into your
business logic. At the edge, one filter maps every result to the right status code,
body shape, and `ProblemDetails` — and the `[ProblemResult]` attribute documents
those error responses in your OpenAPI document automatically.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![CI](https://github.com/gustavorviana/Viana.Results/actions/workflows/ci.yml/badge.svg)](https://github.com/gustavorviana/Viana.Results/actions/workflows/ci.yml)

---

## Why Viana.Results?

- **Domain-first, web-agnostic.** `Result`, `Result<T>`, `ListResult<T>`,
  `PagedResult<T>` and `ProblemResult` live in a `netstandard2.0` core with no
  `Microsoft.AspNetCore.*` reference. Return results from handlers and services;
  translate to HTTP only at the boundary.
- **RFC 9457 by default.** Failures map to ASP.NET Core's native `ProblemDetails`
  and are written as `application/problem+json` — nothing custom on the wire.
- **Self-documenting errors.** `[ProblemResult(404, ...)]` adds the error response
  to your OpenAPI/Swagger/Scalar document, so the docs match what the API actually
  returns. This is the part most Result libraries leave to you.
- **Lists and pagination handled.** `ListResult<T>` serializes as a plain array;
  `PagedResult<T>` keeps its paging metadata — no hand-rolled envelopes.
- **Pick your pieces.** Core, Mediator, MVC, and OpenAPI (native or Swashbuckle)
  ship as independent packages.

---

## Packages

Pick only what you need — every package ships independently.

| Package | NuGet | Downloads | Targets |
|---|---|---|---|
| **Viana.Results** — core types (`Result`, `Result<T>`, `ListResult<T>`, `PagedResult<T>`, `ProblemBuilder`) | [![NuGet](https://img.shields.io/nuget/v/Viana.Results.svg)](https://www.nuget.org/packages/Viana.Results) | [![Downloads](https://img.shields.io/nuget/dt/Viana.Results.svg)](https://www.nuget.org/packages/Viana.Results) | `netstandard2.0`, `net8.0`, `net10.0` |
| **Viana.Results.Mediators** — minimal Mediator with `Activity` telemetry | [![NuGet](https://img.shields.io/nuget/v/Viana.Results.Mediators.svg)](https://www.nuget.org/packages/Viana.Results.Mediators) | [![Downloads](https://img.shields.io/nuget/dt/Viana.Results.Mediators.svg)](https://www.nuget.org/packages/Viana.Results.Mediators) | `netstandard2.0`, `net6.0`, `net8.0`, `net10.0` |
| **Viana.Results.Mvc** — maps `IResult` to native ASP.NET Core responses (`ProblemDetails` on errors) via a global filter | [![NuGet](https://img.shields.io/nuget/v/Viana.Results.Mvc.svg)](https://www.nuget.org/packages/Viana.Results.Mvc) | [![Downloads](https://img.shields.io/nuget/dt/Viana.Results.Mvc.svg)](https://www.nuget.org/packages/Viana.Results.Mvc) | `net6.0`, `net8.0`, `net10.0` |
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

**1. Return results from your domain/application layer** — no ASP.NET here:

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

**2. Register the filter once** (with `Viana.Results.Mvc`):

```csharp
builder.Services.AddControllers().AddVianaResultFilter();
```

**3. Return `IResult` straight from your actions** — the filter writes the status
code and body (mapping errors to `ProblemDetails`), and `[ProblemResult]` documents
the error responses in OpenAPI (with `Viana.Results.OpenApi` or `.Swashbuckle`):

```csharp
[HttpGet("{id}")]
[ProblemResult(404, Title = "User not found")]   // shows up in your OpenAPI document
public async Task<IResult> Get(int id) => await _service.GetUserAsync(id);
```

Prefer XML too? Opt in per app — errors still stay RFC 9457:

```csharp
builder.Services.AddControllers()
    .AddXmlSerializerFormatters()
    .AddVianaResultFilter(o => o.ProblemContentTypes.Add("application/problem+xml"));
```

---

## Documentation

Each package has its own README with full API surface, registration, and
examples:

- [Viana.Results](src/Viana.Results/README.md) — core result types, `Results` factory, `ProblemBuilder`
- [Viana.Results.Mediators](src/Viana.Results.Mediators/README.md) — `IRequest<T>`, `IHandler<,>`, DI registration, Activity tags
- [Viana.Results.Mvc](src/Viana.Results.Mvc/README.md) — `VianaResultFilter`, `VianaResultMvcOptions`, `ProblemResult → ProblemDetails` mapping
- [Viana.Results.OpenApi](src/Viana.Results.OpenApi/README.md) — native OpenAPI transformers, `[ProblemResult]`, `[ResponseExample]`
- [Viana.Results.OpenApi.Swashbuckle](src/Viana.Results.OpenApi.Swashbuckle/README.md) — same surface for `SwaggerGen`

---

## Contributing

Issues and pull requests are welcome at
[`gustavorviana/Viana.Results`](https://github.com/gustavorviana/Viana.Results).

## License

[MIT](LICENSE)
