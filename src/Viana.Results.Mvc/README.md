## Viana.Results.Mvc

Integrates `Viana.Results` with **ASP.NET Core MVC**: a global result filter turns
any `IResult` returned from an action into a native ASP.NET Core response — the
correct HTTP status code, the payload written by the framework's own formatters,
and **RFC 9457 `ProblemDetails`** on errors.

---

### NuGet package

```bash id="3h0peu"
dotnet add package Viana.Results.Mvc
```

### Supported frameworks

* `net6.0`
* `net8.0`
* `net10.0`

---

### Register the filter and return `IResult`

```csharp id="c6qq2u"
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mvc;

// Registers VianaResultFilter: every IResult returned from an action is
// converted to a native response (ProblemDetails on error).
builder.Services.AddControllers().AddVianaResultFilter();
```

```csharp id="l37j1a"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Viana.Results;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IResult> GetUser(int id)
    {
        // On success → the payload with status 200/201/…;
        // on failure → application/problem+json with the mapped status.
        return await userService.GetUserByIdAsync(id);
    }

    [HttpPost]
    public async Task<IResult> CreateUser(CreateUserDto dto)
    {
        return await userService.CreateAsync(dto);
    }
}
```

---

### How results are written

| Result | Response |
|---|---|
| Failure (`Problem` set) | `ProblemDetails` as `application/problem+json`, with the result's status code |
| `Result<T>` success | the payload, status from the result |
| `ListResult<T>` success | the inner array (unwrapped) |
| `PagedResult<T>` success | the paged wrapper (keeps `PageNumber`/`TotalPages`) |
| No body (e.g. `NoContent()`, bare `Ok()`) | status code only |

The `ProblemResult` is mapped to `Microsoft.AspNetCore.Mvc.ProblemDetails`: the
`description` extension becomes the RFC 9457 `detail` member, `instance` becomes
`Instance`, and any other extensions (e.g. `errors`) are copied to `Extensions`.

---

### Options

Configure the problem content types offered for negotiation (default
`application/problem+json`):

```csharp
// Also negotiate XML when an XML formatter is registered
builder.Services.AddControllers()
    .AddXmlSerializerFormatters()
    .AddVianaResultFilter(o => o.ProblemContentTypes.Add("application/problem+xml"));
```
