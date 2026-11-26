using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Mediators.Dispatch;

namespace Viana.Results.Mediators.Tests;

/// <summary>Feature 009 US1: process-wide typed dispatch, correct handler, errors through the task.</summary>
public class DispatchTests
{
    public sealed class A { public string Name { get; set; } = "a"; }
    public sealed class B { public int Number { get; set; } = 2; }

    public sealed class Dual : IRequest<Result<A>>, IRequest<Result<B>> { }
    public sealed class DualA : IHandler<Dual, Result<A>> { public Task<Result<A>> Handle(Dual r, CancellationToken ct = default) => Task.FromResult(Results.Ok(new A())); }
    public sealed class DualB : IHandler<Dual, Result<B>> { public Task<Result<B>> Handle(Dual r, CancellationToken ct = default) => Task.FromResult(Results.Ok(new B())); }

    public sealed class Throws : IRequest<Result> { }
    public sealed class ThrowsHandler : IHandler<Throws, Result>
    {
        public async Task<Result> Handle(Throws r, CancellationToken ct = default) { await Task.Yield(); throw new InvalidTimeZoneException("boom"); }
    }

    public sealed class ThrowsSync : IRequest<Result> { }
    public sealed class ThrowsSyncHandler : IHandler<ThrowsSync, Result>
    {
        public Task<Result> Handle(ThrowsSync r, CancellationToken ct = default) => throw new InvalidTimeZoneException("sync");
    }

    public sealed class NullTask : IRequest<Result> { }
    public sealed class NullTaskHandler : IHandler<NullTask, Result> { public Task<Result> Handle(NullTask r, CancellationToken ct = default) => null!; }

    public sealed class Missing : IRequest<Result> { }

    public class BaseRequest : IRequest<Result> { }
    public sealed class DerivedRequest : BaseRequest { }
    public sealed class BaseHandler : IHandler<BaseRequest, Result> { public Task<Result> Handle(BaseRequest r, CancellationToken ct = default) => Task.FromResult(Results.Ok()); }

    public sealed class Echo : IRequest<Result<int>> { public int Value { get; init; } }
    public sealed class EchoHandler : IHandler<Echo, Result<int>> { public async Task<Result<int>> Handle(Echo r, CancellationToken ct = default) { await Task.Yield(); return Results.Ok(r.Value); } }

    private static ServiceProvider Provider()
    {
        var services = new ServiceCollection();
        services.AddScoped<IHandler<Dual, Result<A>>, DualA>();
        services.AddScoped<IHandler<Dual, Result<B>>, DualB>();
        services.AddScoped<IHandler<Throws, Result>, ThrowsHandler>();
        services.AddScoped<IHandler<ThrowsSync, Result>, ThrowsSyncHandler>();
        services.AddScoped<IHandler<NullTask, Result>, NullTaskHandler>();
        services.AddScoped<IHandler<BaseRequest, Result>, BaseHandler>();
        services.AddScoped<IHandler<Echo, Result<int>>, EchoHandler>();
        return services.BuildServiceProvider();
    }

    private static IMediator NewMediator(ServiceProvider root) => new Mediator(root.CreateScope().ServiceProvider);

    [Fact]
    public async Task RequestWithTwoResultTypes_ReachesEachHandler()
    {
        using var root = Provider();
        var mediator = NewMediator(root);

        var a = await mediator.SendAsync<Result<A>>(new Dual());
        var b = await mediator.SendAsync<Result<B>>(new Dual());

        Assert.Equal("a", a.Data!.Name);
        Assert.Equal(2, b.Data!.Number);
    }

    [Fact]
    public async Task DispatchFromNewScopes_ReusesTheCachedEntry()
    {
        using var root = Provider();
        await NewMediator(root).SendAsync(new Echo { Value = 1 });
        var first = DispatchCache<Result<int>>.Get(typeof(Echo));

        await NewMediator(root).SendAsync(new Echo { Value = 2 });

        Assert.Same(first, DispatchCache<Result<int>>.Get(typeof(Echo)));
    }

    [Fact]
    public async Task HandlerException_PropagatesUnchanged()
    {
        using var root = Provider();

        var ex = await Assert.ThrowsAsync<InvalidTimeZoneException>(() => NewMediator(root).SendAsync(new Throws()));

        Assert.Equal("boom", ex.Message);
        Assert.Contains(nameof(ThrowsHandler), ex.StackTrace);
    }

    [Fact]
    public async Task NullTask_FailsWithHandlerAndRequestNames()
    {
        using var root = Provider();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => NewMediator(root).SendAsync(new NullTask()));

        Assert.Contains(nameof(NullTaskHandler), ex.Message);
        Assert.Contains(nameof(NullTask), ex.Message);
    }

    [Fact]
    public async Task MissingHandler_NamesRequestAndResult()
    {
        using var root = Provider();

        var ex = await Assert.ThrowsAsync<HandlerNotFoundException>(() => NewMediator(root).SendAsync(new Missing()));

        Assert.Equal(typeof(Missing), ex.RequestType);
        Assert.Equal(typeof(Result), ex.ResultType);
        Assert.Contains(nameof(Missing), ex.Message);
        Assert.DoesNotContain("IHandler", ex.Message);
    }

    [Fact]
    public async Task DerivedRequest_IsNotResolvedToBaseHandler()
    {
        using var root = Provider();

        var ex = await Assert.ThrowsAsync<HandlerNotFoundException>(() => NewMediator(root).SendAsync(new DerivedRequest()));

        Assert.Equal(typeof(DerivedRequest), ex.RequestType);
    }

    [Fact]
    public void Errors_SurfaceThroughTheTask_NeverAtCallTime()
    {
        using var root = Provider();
        var mediator = NewMediator(root);

        var nullRequest = mediator.SendAsync<Result>(null!);
        var missing = mediator.SendAsync(new Missing());
        var nullTask = mediator.SendAsync(new NullTask());
        var sync = mediator.SendAsync(new ThrowsSync());

        Assert.IsType<ArgumentNullException>(nullRequest.Exception!.InnerException);
        Assert.IsType<HandlerNotFoundException>(missing.Exception!.InnerException);
        Assert.IsType<InvalidOperationException>(nullTask.Exception!.InnerException);
        Assert.IsType<InvalidTimeZoneException>(sync.Exception!.InnerException);
    }

    [Fact]
    public async Task ConcurrentFirstDispatch_ReturnsCorrectResults()
    {
        using var root = Provider();
        var tasks = Enumerable.Range(0, 64).Select(i => NewMediator(root).SendAsync(new Echo { Value = i }));

        var results = await Task.WhenAll(tasks);

        Assert.Equal(Enumerable.Range(0, 64), results.Select(r => r.Data));
    }
}
