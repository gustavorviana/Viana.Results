# Viana.Results

Biblioteca .NET para padronizar respostas de operações usando o padrão **Result**,
com suporte a coleções, paginação, **Mediator**, integração com **ASP.NET Core**
e integração com **OpenAPI / Swashbuckle**.

---

## Pacotes NuGet

O projeto é distribuído em quatro pacotes independentes:

- **Core**
  - `Viana.Results`
  - Tipos de resultado (`Result`, `Result<T>`, `CollectionResult<T>`,
    `PaginatedResult<T>`), fábrica de resultados (`Results`) e helpers
    para padronizar respostas de sucesso/erro.

- **Mediator**
  - `Viana.Results.Mediators`
  - Implementação simples de Mediator com `IRequest<T>`, `IHandler<TRequest, TResponse>`
    e `IMediator`, incluindo geração de `Activity` no .NET 8+ para observabilidade.

- **ASP.NET Core MVC**
  - `Viana.Results.Mvc`
  - Extensões para converter `Result` em `ActionResult`, filtro global
    (`VianaResultFilter`) e helpers para APIs ASP.NET Core.

- **OpenAPI / Swashbuckle**
  - `Viana.Results.OpenApi.Swashbuckle`
  - Filtros para ajustar schemas e respostas no Swagger, desenhados para
    funcionar em conjunto com `Viana.Results` e `Swashbuckle.AspNetCore`.

---

## Instalação

Escolha os pacotes de acordo com a sua necessidade:

```bash
# Core (obrigatório na maioria dos cenários)
dotnet add package Viana.Results

# Mediator
dotnet add package Viana.Results.Mediators

# ASP.NET Core MVC integration
dotnet add package Viana.Results.Mvc

# OpenAPI / Swashbuckle integration
dotnet add package Viana.Results.OpenApi.Swashbuckle
```

---

## Frameworks Suportados

Dependendo do pacote:

- `Viana.Results`
  - `netstandard2.0`, `net5.0`, `net8.0`, `net10.0`
- `Viana.Results.Mediators`
  - `netstandard2.0`, `net5.0`, `net6.0`, `net8.0`, `net10.0`
- `Viana.Results.Mvc`
  - `net6.0`, `net8.0`, `net10.0`
- `Viana.Results.OpenApi.Swashbuckle`
  - `net8.0`, `net10.0`

---

## Uso básico

### 1. Padrão Result (Core – `Viana.Results`)

O padrão **Result** representa o resultado de uma operação de forma clara e segura.

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
        return Results.Failure(422, "Input cannot be empty");

    return Results.Success("Validation passed");
}

// Different error types
public Result ProcessRequest()
{
    // Not Found (404)
    return Results.NotFound("Resource not found");

    // Bad Request (400)
    // return Results.BadRequest("Invalid request");

    // Unauthorized (401)
    // return Results.Unauthorized("Unauthorized access");

    // Forbidden (403)
    // return Results.Forbidden("Forbidden access");

    // Conflict (409)
    // return Results.Conflict("Conflict detected");

    // Business Rule (400)
    // return Results.BusinessRuleViolated("Business rule violated");
}
```

#### 1.1. Validação

```csharp
using Viana.Results;

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

#### 1.2. Coleções e paginação

```csharp
using Viana.Results;

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

---

### 2. Mediator (`Viana.Results.Mediators`)

O pacote `Viana.Results.Mediators` fornece uma implementação simples do padrão
**Mediator**, integrada com `Result`.

#### 2.1. Definindo Request e Handler

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

#### 2.2. Registrando o Mediator

```csharp
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mediators;

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

#### 2.3. Usando o Mediator

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

        // ASP.NET Core integration (from Viana.Results.Mvc)
        return result.ToActionResult();
    }
}
```

No .NET 8+, o `Mediator` também cria `Activity` automaticamente para facilitar
telemetria (com tags como `request.type`, `result.type`, `handler.type`,
`result.success`, `result.statusCode`).

---

### 3. Integração ASP.NET Core MVC (`Viana.Results.Mvc`)

O pacote `Viana.Results.Mvc` facilita o uso de `Result` em APIs ASP.NET Core.

#### 3.1. Convertendo `Result` em `ActionResult`

```csharp
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

#### 3.2. Filtro global (`VianaResultFilter`)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mvc;
using Viana.Results.Mvc.Filters;

// Registers VianaResultFilter so IResult is converted to JSON responses
services.AddControllers().AddVianaResultFilter();

// Or register the filter manually:
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

---

### 4. OpenAPI / Swashbuckle (`Viana.Results.OpenApi.Swashbuckle`)

O pacote `Viana.Results.OpenApi.Swashbuckle` fornece filtros para o Swagger
compatíveis com `Viana.Results`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Viana.Results.OpenApi.Swashbuckle;

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Registers all Viana.Results filters in one call
    options.AddVianaResultFilters();
});
```

Os filtros incluídos ajustam schemas de erro, removem schemas intermediários de
`Result<T>` e melhoram a documentação das respostas retornadas pela API.

---

## Métodos disponíveis em `Results`

Alguns dos métodos mais importantes da classe estática `Results`:

| Método | Status Code | Descrição |
|--------|-------------|-----------|
| `Success(string message = "Ok")` | 200 | Operação bem-sucedida com mensagem |
| `Success(object data)` | 200 | Operação bem-sucedida com dado |
| `Success(string message, object data)` | 200 | Sucesso com mensagem e dado |
| `Failure(string message, HttpStatusCode statusCode)` | Custom | Falha genérica com status customizado |
| `Failure(ResultError error, string message = null, object data = null, HttpStatusCode statusCode = 422)` | 422 | Falha com detalhes de erro |
| `Failure(Exception exception, HttpStatusCode statusCode = 500)` | 500 | Falha a partir de exceção |
| `NotFound(string message = "The requested resource was not found.")` | 404 | Recurso não encontrado |
| `BadRequest(string message = "Bad request", object data = null)` | 400 | Requisição inválida |
| `Unauthorized(string message = "Unauthorized access.")` | 401 | Não autorizado |
| `Forbidden(string message = "Forbidden access.")` | 403 | Acesso proibido |
| `Conflict(string message = "Conflict occurred.")` | 409 | Conflito |
| `BusinessRuleViolated(string message, object data = null)` | 422 | Regra de negócio violada |
| `Validation(Dictionary<string, string[]> errors, string message = "Validation failed")` | 400 | Erro de validação (arrays) |
| `Validation(Dictionary<string, List<string>> errors, string message = "Validation failed")` | 400 | Erro de validação (listas) |

---

## Licença

[Especifique aqui a licença do projeto.]

---

## Contribuindo

Contribuições são bem-vindas!  
Abra uma *issue* ou envie um *pull request* no repositório
[`gustavorviana/Viana.Results`](https://github.com/gustavorviana/Viana.Results).

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
        return Results.Failure(422, "Input cannot be empty");

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
using Viana.Results.Mvc;

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

#### 5.2. Use Viana Result Filter

```csharp
using Viana.Results.Mvc;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Registers VianaResultFilter so IResult is converted to JSON responses
        services.AddControllers().AddVianaResultFilter();
    }
}

// Or register the filter manually:
using Viana.Results.Mvc.Filters;

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