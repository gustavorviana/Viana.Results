using System.Net;
using Viana.Results;
using Viana.Results.Mediators;

namespace Viana.Results.Mediators.Tests;

public class MediatorTests
{
    // Test request and handler implementations
    private class TestRequest : IRequest<Result<string>>
    {
        public string Value { get; set; } = "";
    }

    private class TestHandler : IHandler<TestRequest, Result<string>>
    {
        public async Task<Result<string>> Handle(TestRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return $"Processed: {request.Value}";
        }
    }

    private class FailingRequest : IRequest<Result<string>>
    {
    }

    private class FailingHandler : IHandler<FailingRequest, Result<string>>
    {
        public Task<Result<string>> Handle(FailingRequest request, CancellationToken cancellationToken = default)
        {
            var problem = new ProblemResult(500, "Handler failed");
            return Task.FromResult(new Result<string>(default, 500, problem));
        }
    }

    private class ExceptionRequest : IRequest<Result<string>>
    {
    }

    private class ExceptionHandler : IHandler<ExceptionRequest, Result<string>>
    {
        public Task<Result<string>> Handle(ExceptionRequest request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Handler threw exception");
        }
    }

    private class RequestWithNonTypedResult : IRequest<Result>
    {
    }

    private class HandlerWithNonTypedResult : IHandler<RequestWithNonTypedResult, Result>
    {
        public Task<Result> Handle(RequestWithNonTypedResult request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Result(200));
        }
    }

    // Simple test service provider
    private class TestServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        public void RegisterService(Type type, object instance)
        {
            _services[type] = instance;
        }

        public object? GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            return service;
        }
    }

    [Fact]
    public async Task Mediator_SendAsync_WithValidHandler_ReturnsResult()
    {
        // Arrange
        var serviceProvider = new TestServiceProvider();
        var handler = new TestHandler();
        serviceProvider.RegisterService(typeof(IHandler<TestRequest, Result<string>>), handler);

        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest { Value = "Test" };

        // Act
        var result = await mediator.SendAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Processed: Test", result.Data);
        Assert.Equal(200, result.Status);
        Assert.Null(result.Problem);
    }

    [Fact]
    public async Task Mediator_SendAsync_WithHandlerNotFound_ThrowsHandlerNotFoundException()
    {
        // Arrange
        var serviceProvider = new TestServiceProvider();
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest { Value = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
        {
            await mediator.SendAsync(request);
        });
    }

    [Fact]
    public async Task Mediator_SendAsync_WithFailingHandler_ReturnsErrorResult()
    {
        // Arrange
        var serviceProvider = new TestServiceProvider();
        var handler = new FailingHandler();
        serviceProvider.RegisterService(typeof(IHandler<FailingRequest, Result<string>>), handler);

        var mediator = new Mediator(serviceProvider);
        var request = new FailingRequest();

        // Act
        var result = await mediator.SendAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.Status);
        Assert.NotNull(result.Problem);
        Assert.Equal("Handler failed", result.Problem.Title);
    }

    [Fact]
    public async Task Mediator_SendAsync_WithHandlerThrowingException_ThrowsException()
    {
        // Arrange
        var serviceProvider = new TestServiceProvider();
        var handler = new ExceptionHandler();
        serviceProvider.RegisterService(typeof(IHandler<ExceptionRequest, Result<string>>), handler);

        var mediator = new Mediator(serviceProvider);
        var request = new ExceptionRequest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await mediator.SendAsync(request);
        });

        Assert.Equal("Handler threw exception", exception.Message);
    }

    [Fact]
    public async Task Mediator_SendAsync_WithHandlerThrowingException_PreservesStackTrace()
    {
        // Arrange
        var serviceProvider = new TestServiceProvider();
        var handler = new ExceptionHandler();
        serviceProvider.RegisterService(typeof(IHandler<ExceptionRequest, Result<string>>), handler);
        var mediator = new Mediator(serviceProvider);
        var request = new ExceptionRequest();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await mediator.SendAsync(request));

        // Assert - stack trace should contain the handler method name (ExceptionHandler.Handle)
        var stackTrace = exception.StackTrace ?? "";
        Assert.Contains("ExceptionHandler", stackTrace);
        Assert.Contains("Handle", stackTrace);
    }

    [Fact]
    public async Task Mediator_SendAsync_CachesHandlerInfo()
    {
        // Arrange
        var serviceProvider = new TestServiceProvider();
        var handler = new TestHandler();
        serviceProvider.RegisterService(typeof(IHandler<TestRequest, Result<string>>), handler);

        var mediator = new Mediator(serviceProvider);
        var request1 = new TestRequest { Value = "First" };
        var request2 = new TestRequest { Value = "Second" };

        // Act
        var result1 = await mediator.SendAsync(request1);
        var result2 = await mediator.SendAsync(request2);

        // Assert
        Assert.Equal("Processed: First", result1.Data);
        Assert.Equal("Processed: Second", result2.Data);
    }

    [Fact]
    public async Task Mediator_SendAsync_WithCancellationToken_PassesTokenToHandler()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tokenReceived = false;

        var serviceProvider = new TestServiceProvider();
        var handler = new CustomHandlerWithToken(async (request, token) =>
        {
            tokenReceived = token != default;
            await Task.CompletedTask;
            return new Result<string>("Done", 200);
        });

        serviceProvider.RegisterService(typeof(IHandler<TestRequest, Result<string>>), handler);

        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest { Value = "Test" };

        // Act
        await mediator.SendAsync(request, cts.Token);

        // Assert
        Assert.True(tokenReceived);
    }

    [Fact]
    public async Task Mediator_SendAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var serviceProvider = new TestServiceProvider();
        var handler = new CustomHandlerWithToken(async (request, token) =>
        {
            token.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            return new Result<string>("Done", 200);
        });
        serviceProvider.RegisterService(typeof(IHandler<TestRequest, Result<string>>), handler);

        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest { Value = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await mediator.SendAsync(request, cts.Token));
    }

    [Fact]
    public async Task Mediator_SendAsync_WithNonTypedResult_ReturnsResultWithoutData()
    {
        // Arrange
        var serviceProvider = new TestServiceProvider();
        var handler = new HandlerWithNonTypedResult();
        serviceProvider.RegisterService(typeof(IHandler<RequestWithNonTypedResult, Result>), handler);

        var mediator = new Mediator(serviceProvider);
        var request = new RequestWithNonTypedResult();

        // Act
        var result = await mediator.SendAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.Null(result.Problem);
    }

    [Fact]
    public async Task Mediator_SendAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceProvider = new TestServiceProvider();
        var mediator = new Mediator(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.SendAsync((IRequest<Result<string>>)null!));
    }

    private class CustomHandlerWithToken : IHandler<TestRequest, Result<string>>
    {
        private readonly Func<TestRequest, CancellationToken, Task<Result<string>>> _handleFunc;

        public CustomHandlerWithToken(Func<TestRequest, CancellationToken, Task<Result<string>>> handleFunc)
        {
            _handleFunc = handleFunc;
        }

        public Task<Result<string>> Handle(TestRequest request, CancellationToken cancellationToken = default)
        {
            return _handleFunc(request, cancellationToken);
        }
    }

    [Fact]
    public void HandlerNotFoundException_Constructor_SetsMessage()
    {
        // Arrange
        var requestType = typeof(TestRequest);

        // Act
        var exception = new HandlerNotFoundException(requestType);

        // Assert
        Assert.Equal($"No handler registered for request type {requestType.Name}", exception.Message);
    }

    [Fact]
    public void HandlerNotFoundException_InheritsFromException()
    {
        // Arrange
        var requestType = typeof(TestRequest);

        // Act
        var exception = new HandlerNotFoundException(requestType);

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void HandlerNotFoundException_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        var inner = new InvalidOperationException("Inner");

        // Act
        var exception = new HandlerNotFoundException(requestType, inner);

        // Assert
        Assert.Same(inner, exception.InnerException);
        Assert.Equal("Inner", exception.InnerException!.Message);
    }
}
