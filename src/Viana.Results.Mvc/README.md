## Viana.Results.Mvc

Returns Viana results from ASP.NET Core MVC on net6.0 and existing apps. On net8.0+, prefer
[Viana.Results.AspNetCore](https://github.com/gustavorviana/Viana.Results/tree/main/src/Viana.Results.AspNetCore).

```bash
dotnet add package Viana.Results.Mvc
```

Targets: `net6.0`, `net8.0`, `net10.0`.

### Global filter

```csharp
using Viana.Results.Mvc;

services.AddControllers().AddVianaResultFilter();

[HttpGet]
public async Task<IResult> GetUsers()
{
    var users = await _userService.GetAllAsync();
    return users;
}
```

### Wrapping a `Result` in an `ActionResult`

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserByIdAsync(id);
    return new VianaResultAction(result);
}
```

Failures are written as `application/problem+json`. `Results.Ok()` without a value answers 204.

Examples: [examples](https://github.com/gustavorviana/Viana.Results/tree/main/examples).
