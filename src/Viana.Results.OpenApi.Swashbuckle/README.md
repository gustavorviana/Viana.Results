## Viana.Results.OpenApi.Swashbuckle

Integration of `Viana.Results` with **OpenAPI / Swashbuckle**, providing filters
to adjust Swagger schemas and responses.

---

### NuGet package

```bash
dotnet add package Viana.Results.OpenApi.Swashbuckle
```

### Supported frameworks

* `net8.0`
* `net10.0`

---

### Registering the filters in Swagger

```csharp
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Viana.Results.OpenApi.Swashbuckle;

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Registers all Viana.Results filters in a single call
    options.AddVianaResultFilters();
});
```

Included filters:

* Adjust error schemas according to `Viana.Results`
* Remove intermediate `Result<T>` schemas
* Improve the documentation of API responses
