using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Viana.Results.Mediators.Tests;

/// <summary>Feature 009 US3: idempotent, fail-fast, robust registration.</summary>
public class RegistrationTests
{
    public sealed class RReq : IRequest<Result> { }
    public sealed class RHandler : IHandler<RReq, Result> { public Task<Result> Handle(RReq r, CancellationToken ct = default) => Task.FromResult(Results.Ok()); }

    /// <summary>Not a handler: used only as the "other" implementation of a duplicate registration.</summary>
    public sealed class OtherImplementation { }

    public abstract class AbstractHandler : IHandler<RReq, Result> { public abstract Task<Result> Handle(RReq r, CancellationToken ct = default); }

    public sealed class GenReq<T> : IRequest<Result> { }
    public sealed class OpenGenericHandler<T> : IHandler<GenReq<T>, Result> { public Task<Result> Handle(GenReq<T> r, CancellationToken ct = default) => Task.FromResult(Results.Ok()); }

    private static int Count<T>(IServiceCollection services) => services.Count(d => d.ServiceType == typeof(T));

    [Fact]
    public void RepeatedRegistration_RegistersOnce()
    {
        var services = new ServiceCollection();
        var assembly = typeof(RegistrationTests).Assembly;

        services.AddMediator(assembly);
        services.AddMediator(assembly);
        services.AddMediator(assembly, typeof(Result).Assembly);

        Assert.Equal(1, Count<IMediator>(services));
        Assert.Equal(1, Count<MediatorOptions>(services));
        Assert.Equal(1, Count<IHandler<RReq, Result>>(services));
    }

    [Fact]
    public void DuplicateHandler_ThrowsNamingBoth()
    {
        var services = new ServiceCollection();
        ((IServiceCollection)services).Add(new ServiceDescriptor(typeof(IHandler<RReq, Result>), typeof(OtherImplementation), ServiceLifetime.Scoped));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ServiceCollectionExtensions.AddMediatorCore(services, null, new[] { typeof(RHandler) }));

        Assert.Contains(nameof(RReq), ex.Message);
        Assert.Contains(nameof(RHandler), ex.Message);
        Assert.Contains(nameof(OtherImplementation), ex.Message);
    }

    [Fact]
    public void AbstractAndOpenGenericTypes_AreSkipped()
    {
        var services = new ServiceCollection();

        ServiceCollectionExtensions.AddMediatorCore(services, null,
            new[] { typeof(AbstractHandler), typeof(OpenGenericHandler<>), typeof(IHandler<RReq, Result>) });

        Assert.DoesNotContain(services, d => d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(IHandler<,>));
        services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true }).Dispose();
    }

    [Fact]
    public void ScannedHandlers_AreScoped()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(RegistrationTests).Assembly);

        Assert.Equal(ServiceLifetime.Scoped, services.Single(d => d.ServiceType == typeof(IHandler<RReq, Result>)).Lifetime);
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    public void HandRegisteredHandler_KeepsItsLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        ((IServiceCollection)services).Add(new ServiceDescriptor(typeof(IHandler<RReq, Result>), typeof(RHandler), lifetime));

        services.AddMediator(typeof(RegistrationTests).Assembly);

        var descriptor = services.Single(d => d.ServiceType == typeof(IHandler<RReq, Result>));
        Assert.Equal(lifetime, descriptor.Lifetime);
    }

    [Fact]
    public void HandRegisteredFactory_IsKept()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHandler<RReq, Result>>(_ => new RHandler());

        services.AddMediator(typeof(RegistrationTests).Assembly);

        var descriptor = services.Single(d => d.ServiceType == typeof(IHandler<RReq, Result>));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.NotNull(descriptor.ImplementationFactory);
    }

    [Fact]
    public void Options_FirstRegistrationWins_AndMediatorUsesThem()
    {
        var services = new ServiceCollection();
        services.AddMediator(o => o.Telemetry.Tracing = false, typeof(RegistrationTests).Assembly);
        services.AddMediator(o => o.Telemetry.Tracing = true, typeof(RegistrationTests).Assembly);

        using var provider = services.BuildServiceProvider();

        Assert.False(provider.GetRequiredService<MediatorOptions>().Telemetry.Tracing);
        Assert.IsType<Mediator>(provider.CreateScope().ServiceProvider.GetRequiredService<IMediator>());
    }

    [Fact]
    public void PartiallyLoadableAssembly_ReturnsLoadableTypes()
    {
        var types = ServiceCollectionExtensions.LoadableTypes(() =>
            throw new ReflectionTypeLoadException(new Type?[] { typeof(RHandler), null }, new Exception?[] { new TypeLoadException() }));

        Assert.Equal(new[] { typeof(RHandler) }, types);
    }
}
