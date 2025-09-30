# Viana.Results

A .NET library for standardizing operation responses with support for the Result pattern, Mediator, and ASP.NET Core integration.

## Features

- ✅ Result pattern for success/error handling
- ✅ Mediator pattern implementation
- ✅ Support for collections and pagination
- ✅ ASP.NET Core integration
- ✅ Implicit conversions for ease of use
- ✅ Support for .NET Standard 2.0 and .NET 8.0

## Basic Usage

### 1. Result Pattern

The Result pattern allows you to represent operation results in a clear and safe way:

```csharp
using Viana.Results;

// Simple success
public Result ProcessData()
{
    return Results.Success("Operation completed successfully");
}

// Success with data
public Result<User> GetUser(int id)
{
    var user = database.FindUser(id);
    return user; // Implicit conversion
}

// Error
public Result ValidateInput(string input)
{
    if (string.IsNullOrEmpty(input))
        return Results.Failure("Input cannot be empty");

    return Results.Success("Validation passed");
}

// Different error types
public Result ProcessRequest()
{
    // Not Found (404)
    return Results.NotFound("Resource not found");

    // Bad Request (400)
    return Results.BadRequest("Invalid request");

    // Unauthorized (401)
    return Results.Unauthorized("Unauthorized access");

    // Forbidden (403)
    return Results.Forbidden("Forbidden access");

    // Conflict (409)
    return Results.Conflict("Conflict detected");

    // Business Rule (400)
    return Results.BusinessRuleViolated("Business rule violated");
}
```

### 2. Result with Validation

```csharp
public Result ValidateUser(User user)
{
    var errors = new Dictionary<string, string[]>
    {
        ["Email"] = new[] { "Email is required", "Invalid email" },
        ["Password"] = new[] { "Password must be at least 8 characters" }
    };

    return Results.Validation(errors, "Validation failed");
}
```

### 3. Result with Collections

```csharp
// Simple collection
public CollectionResult<Product> GetProducts()
{
    var products = database.GetProducts().ToList();
    return products; // Implicit conversion
}

// Paginated result
public PaginatedResult<Product> GetProductsPaged(int page, int pageSize)
{
    var query = database.GetProducts();
    var total = query.Count();
    var items = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    var pages = (int)Math.Ceiling(total / (double)pageSize);

    return new PaginatedResult<Product>(items, total, pages);
}
```

### 4. Mediator Pattern

The Mediator allows you to decouple requests from their handlers:

#### 4.1. Define Request and Handler

```csharp
using Viana.Results.Mediators;

// Define the request
public class GetUserByIdRequest : IRequest<Result<User>>
{
    public int UserId { get; set; }
}

// Define the handler
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

#### 4.2. Register Mediator and Handlers

```csharp
using Viana.Results.Mediators;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register the mediator and all handlers from an assembly
        services.AddMediator(typeof(GetUserByIdHandler).Assembly);

        // Or register from multiple assemblies
        services.AddMediator(
            typeof(GetUserByIdHandler).Assembly,
            typeof(AnotherHandler).Assembly
        );
    }
}
```

#### 4.3. Use the Mediator

```csharp
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var request = new GetUserByIdRequest { UserId = id };
        var result = await _mediator.SendAsync(request);

        return result.ToActionResult(); // ASP.NET Core integration
    }
}
```

### 5. ASP.NET Core Integration

The library offers extensions to facilitate use in ASP.NET Core APIs:

#### 5.1. Convert Result to ActionResult

```csharp
using Viana.Results.AspNetCore;

[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserByIdAsync(id);

    // Automatically converts Result to ActionResult with appropriate status code
    return result.ToActionResult();
}

[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserDto dto)
{
    var result = await _userService.CreateAsync(dto);

    return result.ToActionResult();
}
```

#### 5.2. Use Custom Result Filter

```csharp
using Viana.Results.AspNetCore.Filters;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // Automatically converts IResult to formatted HTTP responses
            options.Filters.Add<CustomResultFilter>();
        });
    }
}

// Now you can return IResult directly
[HttpGet]
public async Task<IResult> GetUsers()
{
    var users = await _userService.GetAllAsync();
    return users; // Will be automatically formatted
}
```

## Available Result Types

### `Results` Class Methods

| Method | Status Code | Description |
|--------|-------------|-------------|
| `Success(string message = "Ok")` | 200 | Successful operation with message |
| `Success(object data)` | 200 | Successful operation with data |
| `Success(string message, object data)` | 200 | Successful operation with message and data |
| `Failure(string message, HttpStatusCode statusCode)` | Custom | Generic failure with custom status code |
| `Failure(ResultError error, string message = null, object data = null, HttpStatusCode statusCode = 422)` | 422 | Failure with error details |
| `Failure(Exception exception, HttpStatusCode statusCode = 500)` | 500 | Failure from exception |
| `NotFound(string message = "The requested resource was not found.")` | 404 | Resource not found |
| `BadRequest(string message = "Bad request", object data = null)` | 400 | Invalid request |
| `Unauthorized(string message = "Unauthorized access.")` | 401 | Unauthorized |
| `Forbidden(string message = "Forbidden access.")` | 403 | Forbidden access |
| `Conflict(string message = "Conflict occurred.")` | 409 | Conflict |
| `BusinessRuleViolated(string message, object data = null)` | 422 | Business rule violated |
| `Validation(Dictionary<string, string[]> errors, string message = "Validation failed")` | 400 | Validation error with string arrays |
| `Validation(Dictionary<string, List<string>> errors, string message = "Validation failed")` | 400 | Validation error with string lists |

## Advanced Examples

### Complete Example with Mediator

```csharp
// Request
public class CreateOrderRequest : IRequest<Result<Order>>
{
    public int UserId { get; set; }
    public List<OrderItem> Items { get; set; }
}

// Handler
public class CreateOrderHandler : IHandler<CreateOrderRequest, Result<Order>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<Order>> Handle(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        // Validate user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Results.NotFound("User not found");

        // Validate items
        if (request.Items == null || !request.Items.Any())
            return Results.BadRequest("Order must contain at least one item");

        // Create order
        var order = new Order
        {
            UserId = request.UserId,
            Items = request.Items,
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepository.CreateAsync(order, cancellationToken);

        return order; // Implicit conversion
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        var request = new CreateOrderRequest
        {
            UserId = dto.UserId,
            Items = dto.Items
        };

        var result = await _mediator.SendAsync(request);
        return result.ToActionResult();
    }
}
```

## Exception Handling

```csharp
public class ExceptionHandler : IHandler<RiskyRequest, Result<Data>>
{
    public async Task<Result<Data>> Handle(
        RiskyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Risky operation
            var data = await DoSomethingRisky();
            return data;
        }
        catch (ValidationException ex)
        {
            return Results.Validation(ex.Errors);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Failure(ex); // Status 500
        }
    }
}
```

## Monitoring (Activity/Telemetry)

In .NET 8.0+, the Mediator automatically creates Activities for observability:

```csharp
// The mediator automatically adds tags:
// - request.type
// - result.type
// - handler.type
// - result.success
// - result.statusCode
```

## License

[Specify your license here]

## Contributing

Contributions are welcome! Please open an issue or pull request.