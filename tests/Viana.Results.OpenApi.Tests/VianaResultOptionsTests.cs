using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Viana.Results.OpenApi.Tests;

public class VianaResultOptionsTests
{
    private sealed class A : IExampleProvider<object> { public object GetExample() => new(); }
    private sealed class B : IExampleProvider<object> { public object GetExample() => new(); }

    [Fact]
    public void AddVianaResultExamples_CalledTwice_ComposesRegistrations()
    {
        var services = new ServiceCollection();
        services.AddVianaResultExamples(o => o.AddExample<A>(500));
        services.AddVianaResultExamples(o => o.AddExample<B>(503));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<VianaResultOptions>>().Value;

        Assert.Equal(new[] { 500, 503 }, options.GlobalExamples.Select(e => e.StatusCode).ToArray());
        Assert.Same(options, provider.GetRequiredService<VianaResultOptions>());
    }
}
