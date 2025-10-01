using System.Net;
using Viana.Results.Mediators;

namespace Viana.Results.Tests
{
    public class MediatorTests
    {
        // Test request and handler implementations
        private class TestRequest : IRequest<Result<string>>
        {
            public string Value { get; set; }
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
                var error = new ResultError("Handler failed");
                return Task.FromResult(new Result<string>(error, HttpStatusCode.InternalServerError));
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

        // Simple test service provider
        private class TestServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _services = new();

            public void RegisterService(Type type, object instance)
            {
                _services[type] = instance;
            }

            public object GetService(Type serviceType)
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
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.Error);
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
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal("Handler failed", result.Error.Message);
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

            // Custom handler that checks for cancellation token
            var handler = new CustomHandlerWithToken(async (request, token) =>
            {
                tokenReceived = token != default;
                await Task.CompletedTask;
                return new Result("Done");
            });

            serviceProvider.RegisterService(typeof(IHandler<TestRequest, Result<string>>), handler);

            var mediator = new Mediator(serviceProvider);
            var request = new TestRequest { Value = "Test" };

            // Act
            await mediator.SendAsync(request, cts.Token);

            // Assert
            Assert.True(tokenReceived);
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
    }
}