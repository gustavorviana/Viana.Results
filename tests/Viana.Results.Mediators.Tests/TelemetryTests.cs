using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Viana.Results.Mediators.Tests;

/// <summary>Feature 009 US2/US4: spans, tags, status rules and exception detail levels.</summary>
[Collection(TelemetryCollection.Name)]
public class TelemetryTests
{
    public sealed class User { }

    public sealed class TOk : IRequest<Result<User>> { }
    public sealed class TOkHandler : IHandler<TOk, Result<User>> { public Task<Result<User>> Handle(TOk r, CancellationToken ct = default) => Task.FromResult(Results.Ok(new User())); }

    public sealed class T404 : IRequest<Result> { }
    public sealed class T404Handler : IHandler<T404, Result> { public Task<Result> Handle(T404 r, CancellationToken ct = default) => Task.FromResult(new ProblemBuilder(404).WithTitle("Missing").BuildResult()); }

    public sealed class T503 : IRequest<Result> { }
    public sealed class T503Handler : IHandler<T503, Result> { public Task<Result> Handle(T503 r, CancellationToken ct = default) => Task.FromResult(new ProblemBuilder(503).WithTitle("Down").BuildResult()); }

    public sealed class TBoom : IRequest<Result> { }
    public sealed class TBoomHandler : IHandler<TBoom, Result> { public Task<Result> Handle(TBoom r, CancellationToken ct = default) => throw new InvalidTimeZoneException("secret-data"); }

    public sealed class TCancel : IRequest<Result> { }
    public sealed class TCancelHandler : IHandler<TCancel, Result> { public Task<Result> Handle(TCancel r, CancellationToken ct = default) => throw new OperationCanceledException(); }

    public sealed class TMissing : IRequest<Result> { }

    public sealed class TFull : IRequest<Result> { }
    public sealed class TFullHandler : IHandler<TFull, Result> { public Task<Result> Handle(TFull r, CancellationToken ct = default) => throw new InvalidTimeZoneException("secret-data"); }
    public sealed class TTypeOnly : IRequest<Result> { }
    public sealed class TTypeOnlyHandler : IHandler<TTypeOnly, Result> { public Task<Result> Handle(TTypeOnly r, CancellationToken ct = default) => throw new InvalidTimeZoneException("secret-data"); }
    public sealed class TNone : IRequest<Result> { }
    public sealed class TNoneHandler : IHandler<TNone, Result> { public Task<Result> Handle(TNone r, CancellationToken ct = default) => throw new InvalidTimeZoneException("secret-data"); }

    public sealed class TPropagation : IRequest<Result> { }
    public sealed class TPropagationHandler : IHandler<TPropagation, Result> { public Task<Result> Handle(TPropagation r, CancellationToken ct = default) => Task.FromResult(Results.Ok()); }

    public sealed class TChild : IRequest<Result> { }
    public sealed class TChildHandler : IHandler<TChild, Result> { public Task<Result> Handle(TChild r, CancellationToken ct = default) => Task.FromResult(Results.Ok()); }

    internal static IMediator Mediator(Action<MediatorOptions>? configure = null, Type? host = null)
    {
        var options = new MediatorOptions();
        configure?.Invoke(options);
        var services = new ServiceCollection();
        foreach (var type in (host ?? typeof(TelemetryTests)).GetNestedTypes())
            foreach (var i in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>)))
                services.AddScoped(i, type);
        return new Mediator(services.BuildServiceProvider().CreateScope().ServiceProvider, options);
    }

    internal static async Task Swallow(Func<Task> send) { try { await send(); } catch { } }

    [Fact]
    public async Task Success_SpanNameKindAndTags()
    {
        using var capture = new TelemetryCapture(nameof(TOk));

        await Mediator().SendAsync(new TOk());

        var span = capture.Single();
        Assert.Equal(ActivityKind.Internal, span.Kind);
        Assert.Equal(nameof(TOk), span.GetTagItem(MediatorDiagnostics.RequestTypeTag));
        Assert.Equal("Result<User>", span.GetTagItem(MediatorDiagnostics.ResultTypeTag));
        Assert.Equal(nameof(TOkHandler), span.GetTagItem(MediatorDiagnostics.HandlerTypeTag));
        Assert.Equal(true, span.GetTagItem(MediatorDiagnostics.SuccessTag));
        Assert.Equal(200, span.GetTagItem(MediatorDiagnostics.StatusTag));
        Assert.Equal(ActivityStatusCode.Ok, span.Status);
    }

    [Fact]
    public async Task ClientProblem_IsOk()
    {
        using var capture = new TelemetryCapture(nameof(T404));

        await Mediator().SendAsync(new T404());

        var span = capture.Single();
        Assert.Equal(ActivityStatusCode.Ok, span.Status);
        Assert.Equal(false, span.GetTagItem(MediatorDiagnostics.SuccessTag));
        Assert.Equal(404, span.GetTagItem(MediatorDiagnostics.StatusTag));
    }

    [Fact]
    public async Task ServerProblem_IsErrorWithTitle()
    {
        using var capture = new TelemetryCapture(nameof(T503));

        await Mediator().SendAsync(new T503());

        var span = capture.Single();
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal("Down", span.StatusDescription);
    }

    [Fact]
    public async Task Exception_IsErrorWithEventAndErrorType()
    {
        using var capture = new TelemetryCapture(nameof(TBoom));

        await Swallow(() => Mediator().SendAsync(new TBoom()));

        var span = capture.Single();
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal(typeof(InvalidTimeZoneException).FullName, span.GetTagItem(MediatorDiagnostics.ErrorTypeTag));
        Assert.Equal(nameof(TBoomHandler), span.GetTagItem(MediatorDiagnostics.HandlerTypeTag));
        var ev = Assert.Single(span.Events, e => e.Name == "exception");
        Assert.Contains(ev.Tags, t => t.Key == "exception.message" && (string?)t.Value == "secret-data");
    }

    [Fact]
    public async Task HandlerNotFound_IsError()
    {
        using var capture = new TelemetryCapture(nameof(TMissing));

        await Swallow(() => Mediator().SendAsync(new TMissing()));

        var span = capture.Single();
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal(typeof(HandlerNotFoundException).FullName, span.GetTagItem(MediatorDiagnostics.ErrorTypeTag));
    }

    [Fact]
    public async Task Cancellation_IsNotError()
    {
        using var capture = new TelemetryCapture(nameof(TCancel));

        await Swallow(() => Mediator().SendAsync(new TCancel()));

        var span = capture.Single();
        Assert.Equal(ActivityStatusCode.Unset, span.Status);
        Assert.Equal(true, span.GetTagItem(MediatorDiagnostics.CancelledTag));
        Assert.DoesNotContain(span.Events, e => e.Name == "exception");
    }

    [Fact]
    public async Task PropagationOnly_BuildsNoTags()
    {
        using var capture = new TelemetryCapture(nameof(TPropagation), metrics: false, sampling: ActivitySamplingResult.PropagationData);

        await Mediator().SendAsync(new TPropagation());

        Assert.Empty(capture.Single().TagObjects);
    }

    [Fact]
    public async Task Span_IsChildOfAmbientActivity()
    {
        using var capture = new TelemetryCapture(nameof(TChild));
        using var parent = new Activity("parent").Start();

        await Mediator().SendAsync(new TChild());

        Assert.Equal(parent.SpanId, capture.Single().ParentSpanId);
    }

    [Fact]
    public async Task DetailFull_RecordsTypeMessageAndStack()
    {
        using var capture = new TelemetryCapture(nameof(TFull));

        await Swallow(() => Mediator().SendAsync(new TFull()));

        var span = capture.Single();
        Assert.Equal("secret-data", span.StatusDescription);
        var tags = Assert.Single(span.Events, e => e.Name == "exception").Tags.ToDictionary(t => t.Key, t => t.Value);
        Assert.Equal(typeof(InvalidTimeZoneException).FullName, tags["exception.type"]);
        Assert.Equal("secret-data", tags["exception.message"]);
        Assert.Contains(nameof(TFullHandler), (string)tags["exception.stacktrace"]!);
    }

    [Fact]
    public async Task DetailTypeOnly_RecordsOnlyTheType()
    {
        using var capture = new TelemetryCapture(nameof(TTypeOnly));

        await Swallow(() => Mediator(o => o.Telemetry.ExceptionDetails = ExceptionDetailLevel.TypeOnly).SendAsync(new TTypeOnly()));

        var span = capture.Single();
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal(typeof(InvalidTimeZoneException).FullName, span.StatusDescription);
        var ev = Assert.Single(span.Events, e => e.Name == "exception");
        Assert.Equal(new[] { "exception.type" }, ev.Tags.Select(t => t.Key).ToArray());
        Assert.DoesNotContain(span.Events.SelectMany(e => e.Tags), t => (t.Value as string)?.Contains("secret-data") == true);
    }

    [Fact]
    public async Task DetailNone_NoEventButError()
    {
        using var capture = new TelemetryCapture(nameof(TNone));

        await Swallow(() => Mediator(o => o.Telemetry.ExceptionDetails = ExceptionDetailLevel.None).SendAsync(new TNone()));

        var span = capture.Single();
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Null(span.StatusDescription);
        Assert.Empty(span.Events);
        Assert.Equal(typeof(InvalidTimeZoneException).FullName, span.GetTagItem(MediatorDiagnostics.ErrorTypeTag));
    }
}
