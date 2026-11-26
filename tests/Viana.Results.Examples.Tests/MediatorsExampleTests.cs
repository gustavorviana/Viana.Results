using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Json;
using Viana.Results.Mediators;

namespace Viana.Results.Examples.Tests;

/// <summary>The Mediators example: statuses and the telemetry each scenario produces.</summary>
[Collection(nameof(TelemetryCollection))]
public sealed class MediatorsExampleTests : IDisposable
{
    private readonly ConcurrentQueue<Activity> _spans = new();
    private readonly ConcurrentQueue<string> _measurements = new();
    private readonly ActivityListener _activities;
    private readonly MeterListener _meters;

    public MediatorsExampleTests()
    {
        _activities = new ActivityListener
        {
            ShouldListenTo = s => s.Name == MediatorDiagnostics.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = a => _spans.Enqueue(a)
        };
        ActivitySource.AddActivityListener(_activities);

        _meters = new MeterListener
        {
            InstrumentPublished = (i, l) => { if (i.Meter.Name == MediatorDiagnostics.MeterName) l.EnableMeasurementEvents(i); }
        };
        _meters.SetMeasurementEventCallback<double>((i, _, _, _) => _measurements.Enqueue(i.Name));
        _meters.Start();
    }

    public void Dispose()
    {
        _activities.Dispose();
        _meters.Dispose();
    }

    private async Task<Activity> SpanFor(string requestType)
    {
        for (var i = 0; i < 100; i++)
        {
            var span = _spans.FirstOrDefault(a => (a.GetTagItem(MediatorDiagnostics.RequestTypeTag) as string) == requestType);
            if (span is not null)
                return span;
            await Task.Delay(50);
        }
        throw new Xunit.Sdk.XunitException($"no span for {requestType}");
    }

    [Fact]
    public async Task Crud_goes_through_the_mediator_with_span_and_metric()
    {
        var client = Hosts.CreateMediators();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/products")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/products/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/products/99")).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/products", Hosts.ValidProduct())).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/products", Hosts.ValidProduct())).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync("/products/2", Hosts.ValidProduct("MS-002"))).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await client.DeleteAsync("/products/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/products/3")).StatusCode);

        var span = await SpanFor("GetProduct");
        Assert.Equal("Mediator.Send GetProduct", span.DisplayName);
        Assert.Equal("Result<Product>", span.GetTagItem(MediatorDiagnostics.ResultTypeTag));
        Assert.Equal("GetProductHandler", span.GetTagItem(MediatorDiagnostics.HandlerTypeTag));
        Assert.Contains(MediatorDiagnostics.DurationInstrument, _measurements);
    }

    [Fact]
    public async Task Throwing_handler_is_a_500_problem_and_an_error_span()
    {
        var response = await Hosts.CreateMediators().GetAsync("/scenarios/throws");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var span = await SpanFor("ThrowingRequest");
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal(typeof(InvalidOperationException).FullName, span.GetTagItem(MediatorDiagnostics.ErrorTypeTag));
        var exception = Assert.Single(span.Events, e => e.Name == "exception");
        Assert.Contains(exception.Tags, t => t.Key == "exception.message");
    }

    [Fact]
    public async Task TypeOnly_detail_level_drops_the_exception_message()
    {
        await Hosts.CreateMediators(("Mediator:Telemetry:ExceptionDetails", "TypeOnly")).GetAsync("/scenarios/throws");

        var exception = Assert.Single((await SpanFor("ThrowingRequest")).Events, e => e.Name == "exception");
        Assert.Contains(exception.Tags, t => t.Key == "exception.type");
        Assert.DoesNotContain(exception.Tags, t => t.Key is "exception.message" or "exception.stacktrace");
    }

    [Fact]
    public async Task Unregistered_request_reports_what_was_asked_and_an_error_span()
    {
        var response = await Hosts.CreateMediators().GetAsync("/scenarios/unregistered");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("UnregisteredRequest", (await response.Json()).GetProperty("detail").GetString());
        var span = await SpanFor("UnregisteredRequest");
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal(typeof(HandlerNotFoundException).FullName, span.GetTagItem(MediatorDiagnostics.ErrorTypeTag));
    }

    [Fact]
    public async Task Unavailable_dependency_is_a_503_and_an_error_span()
    {
        var response = await Hosts.CreateMediators().GetAsync("/scenarios/unavailable");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var span = await SpanFor("UnavailableRequest");
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal(503, span.GetTagItem(MediatorDiagnostics.StatusTag));
    }

    [Fact]
    public async Task Cancelled_request_is_not_an_error()
    {
        var client = Hosts.CreateMediators();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetAsync("/scenarios/slow?seconds=10", cts.Token));

        var span = await SpanFor("SlowRequest");
        Assert.NotEqual(ActivityStatusCode.Error, span.Status);
        Assert.Equal(true, span.GetTagItem(MediatorDiagnostics.CancelledTag));
    }

    [Fact]
    public async Task Disabled_telemetry_produces_nothing()
    {
        var client = Hosts.CreateMediators(("Mediator:Telemetry:Tracing", "false"), ("Mediator:Telemetry:Metrics", "false"));

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/products/1")).StatusCode);
        await Task.Delay(200);

        Assert.Empty(_spans);
        Assert.Empty(_measurements);
    }
}
