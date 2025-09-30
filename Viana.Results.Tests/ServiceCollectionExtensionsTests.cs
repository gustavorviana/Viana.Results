using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mediators;

namespace Viana.Results.Tests
{
    public class ServiceCollectionExtensionsTests
    {
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

        private class AnotherRequest : IRequest<Result<int>>
        {
            public int Number { get; set; }
        }

        private class AnotherHandler : IHandler<AnotherRequest, Result<int>>
        {
            public async Task<Result<int>> Handle(AnotherRequest request, CancellationToken cancellationToken = default)
            {
                await Task.CompletedTask;
                return request.Number * 2;
            }
        }

        [Fact]
        public void AddMediator_WithSingleAssembly_RegistersMediatorAndHandlers()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            services.AddMediator(assembly);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator = serviceProvider.GetService<IMediator>();
            Assert.NotNull(mediator);
            Assert.IsType<Mediator>(mediator);

            var testHandler = serviceProvider.GetService<IHandler<TestRequest, Result<string>>>();
            Assert.NotNull(testHandler);
            Assert.IsType<TestHandler>(testHandler);

            var anotherHandler = serviceProvider.GetService<IHandler<AnotherRequest, Result<int>>>();
            Assert.NotNull(anotherHandler);
            Assert.IsType<AnotherHandler>(anotherHandler);
        }

        [Fact]
        public void AddMediator_WithMultipleAssemblies_RegistersMediatorAndHandlers()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly1 = Assembly.GetExecutingAssembly();
            var assembly2 = typeof(IMediator).Assembly;

            // Act
            services.AddMediator(assembly1, assembly2);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator = serviceProvider.GetService<IMediator>();
            Assert.NotNull(mediator);
            Assert.IsType<Mediator>(mediator);

            var testHandler = serviceProvider.GetService<IHandler<TestRequest, Result<string>>>();
            Assert.NotNull(testHandler);
        }

        [Fact]
        public void AddMediator_WithNullServices_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceCollection services = null;
            var assembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddMediator(assembly));
        }

        [Fact]
        public void AddMediator_WithNullAssembly_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            Assembly assembly = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddMediator(assembly));
        }

        [Fact]
        public void AddMediator_WithNullAssembliesArray_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();
            Assembly[] assemblies = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => services.AddMediator(assemblies));
        }

        [Fact]
        public void AddMediator_WithEmptyAssembliesArray_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();
            var assemblies = Array.Empty<Assembly>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => services.AddMediator(assemblies));
        }

        [Fact]
        public void AddMediator_RegistersHandlersAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            services.AddMediator(assembly);

            // Assert
            var mediatorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMediator));
            Assert.NotNull(mediatorDescriptor);
            Assert.Equal(ServiceLifetime.Scoped, mediatorDescriptor.Lifetime);

            var handlerDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHandler<TestRequest, Result<string>>));
            Assert.NotNull(handlerDescriptor);
            Assert.Equal(ServiceLifetime.Scoped, handlerDescriptor.Lifetime);
        }

        [Fact]
        public async Task AddMediator_IntegrationTest_CanSendRequestThroughMediator()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMediator(Assembly.GetExecutingAssembly());
            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var request = new TestRequest { Value = "Integration Test" };

            // Act
            var result = await mediator.SendAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Processed: Integration Test", result.Data);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task AddMediator_WithMultipleHandlers_CanResolveAllHandlers()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMediator(Assembly.GetExecutingAssembly());
            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();

            // Act
            var request1 = new TestRequest { Value = "Test1" };
            var result1 = await mediator.SendAsync(request1);

            var request2 = new AnotherRequest { Number = 5 };
            var result2 = await mediator.SendAsync(request2);

            // Assert
            Assert.Equal("Processed: Test1", result1.Data);
            Assert.Equal(10, result2.Data);
        }

        [Fact]
        public void AddMediator_SkipsAbstractAndInterfaceTypes()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            services.AddMediator(assembly);
            var serviceProvider = services.BuildServiceProvider();

            // Assert - Should only have concrete handler implementations
            var handlerDescriptors = services.Where(d =>
                d.ServiceType.IsGenericType &&
                d.ServiceType.GetGenericTypeDefinition() == typeof(IHandler<,>));

            foreach (var descriptor in handlerDescriptors)
            {
                Assert.NotNull(descriptor.ImplementationType);
                Assert.False(descriptor.ImplementationType.IsAbstract);
                Assert.False(descriptor.ImplementationType.IsInterface);
            }
        }

        [Fact]
        public void AddMediator_WithMultipleAssemblies_RegistersMediatorOnlyOnce()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly1 = Assembly.GetExecutingAssembly();
            var assembly2 = typeof(IMediator).Assembly;

            // Act
            services.AddMediator(assembly1, assembly2);

            // Assert
            var mediatorDescriptors = services.Where(d => d.ServiceType == typeof(IMediator)).ToList();
            Assert.Single(mediatorDescriptors);
        }

        [Fact]
        public void AddMediator_WithAssemblyContainingNullInArray_SkipsNullAssembly()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            services.AddMediator(assembly, null, assembly);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator = serviceProvider.GetService<IMediator>();
            Assert.NotNull(mediator);
        }

        [Fact]
        public void AddMediator_ReturnsServiceCollection_ForChaining()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = services.AddMediator(assembly);

            // Assert
            Assert.Same(services, result);
        }
    }
}