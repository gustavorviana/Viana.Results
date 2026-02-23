## Viana.Results.Mediators

A simple **Mediator** pattern implementation integrated with `Viana.Results`,
allowing orchestration of *requests* and *handlers* returning `Result`.

---

### NuGet package

```bash
dotnet add package Viana.Results.Mediators
```

### Supported frameworks

* `netstandard2.0`
* `net5.0`
* `net6.0`
* `net8.0`
* `net10.0`

---

### Defining Request and Handler

```csharp
using Viana.Results;
using Viana.Results.Mediators;

// Request
public class GetUserByIdRequest : IRequest<Result<User>>
{
    public int UserId { get; set; }
}

// Handler
public class GetUserByIdHandler : IHandler<GetUserByIdRequest, Result<User>>
{
    private readonly IUserRepository _repository;

    public GetUserByIdHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<User>> Handle(
        GetUserByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
            return Results.NotFound("User not found");

        return user; // Implicit conversion to Result<User>
    }
}
```

---

### Registering the Mediator

```csharp
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mediators;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Registers the mediator and all handlers from an assembly
        services.AddMediator(typeof(GetUserByIdHandler).Assembly);

        // Or from multiple assemblies
        services.AddMediator(
            typeof(GetUserByIdHandler).Assembly,
            typeof(AnotherHandler).Assembly
        );
    }
}
```

---

### Usage in a Controller (with MVC)

```csharp
using Microsoft.AspNetCore.Mvc;
using Viana.Results;
using Viana.Results.Mediators;
using Viana.Results.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var request = new GetUserByIdRequest { UserId = id };
        var result = await _mediator.SendAsync(request);

        // ASP.NET Core integration (Viana.Results.Mvc)
        return result.ToActionResult();
    }
}
```

---

### Telemetry and Activity (.NET 8+)

In .NET 8+, the `IMediator` automatically creates an `Activity` to improve
observability, including tags such as:

* `request.type`
* `result.type`
* `handler.type`
* `result.success`
* `result.statusCode`
