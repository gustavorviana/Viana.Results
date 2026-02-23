## Viana.Results.Mvc

Extensions to integrate `Viana.Results` with **ASP.NET Core MVC**, allowing you to
convert `Result` into `ActionResult` and use a global filter to format responses.

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

### Converting `Result` to `ActionResult`

```csharp id="l37j1a"
using Microsoft.AspNetCore.Mvc;
using Viana.Results;
using Viana.Results.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);

        // Converts Result to ActionResult with the appropriate status code
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return result.ToActionResult();
    }
}
```

---

### Global filter (`VianaResultFilter`)

```csharp id="c6qq2u"
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mvc;
using Viana.Results.Mvc.Filters;

// Registers VianaResultFilter so that IResult is converted into JSON responses
services.AddControllers().AddVianaResultFilter();

// Or manual registration:
services.AddControllers(options =>
{
    options.Filters.Add<VianaResultFilter>();
});

// Now you can return IResult directly
[HttpGet]
public async Task<IResult> GetUsers()
{
    var users = await _userService.GetAllAsync();
    return users; // Will be automatically formatted
}
```
