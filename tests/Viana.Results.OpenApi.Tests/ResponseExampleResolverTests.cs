using System.Reflection;
using Viana.Results.OpenApi;

namespace Viana.Results.OpenApi.Tests;

public class ResponseExampleResolverTests
{
    public sealed class FakeProvider : IExampleProvider<object>
    {
        public object GetExample() => new { ok = true };
    }

    public sealed class AnotherProvider : IExampleProvider<object>
    {
        public object GetExample() => new { ok = false };
    }

    public sealed class ProviderWithCtorDependency : IExampleProvider<string>
    {
        public ProviderWithCtorDependency(string injected) { Injected = injected; }
        public string Injected { get; }
        public string GetExample() => Injected;
    }

    // ----- test fixtures: controllers and methods used to read attributes via reflection -----

    [ResponseExample(500, typeof(FakeProvider))]
    class ClassWithExample
    {
        public void MethodWithoutOwn() { }

        [ResponseExample(404, typeof(AnotherProvider))]
        public void MethodWithDifferentStatus() { }

        [ResponseExample(500, typeof(AnotherProvider))]
        public void MethodOverridesClassForSameStatus() { }
    }

    class ClassWithoutExample
    {
        [ResponseExample(400, typeof(FakeProvider), Name = "default")]
        [ResponseExample(400, typeof(AnotherProvider), Name = "edge")]
        public void MultiNamedSameStatus() { }
    }

    private static MethodInfo MethodOf(string name) =>
        typeof(ClassWithExample).GetMethod(name)
        ?? typeof(ClassWithoutExample).GetMethod(name)
        ?? throw new InvalidOperationException("missing method " + name);

    [Fact]
    public void Collect_NoAttributes_ReturnsEmpty()
    {
        // Use a method on a class with no class-level attribute either
        var method = typeof(ResponseExampleResolverTests).GetMethod(nameof(Collect_NoAttributes_ReturnsEmpty))!;
        var result = ResponseExampleResolver.Collect(method, Array.Empty<GlobalExampleRegistration>());
        Assert.Empty(result);
    }

    [Fact]
    public void Collect_PicksUpClassLevelAttribute()
    {
        var method = MethodOf(nameof(ClassWithExample.MethodWithoutOwn));
        var result = ResponseExampleResolver.Collect(method, Array.Empty<GlobalExampleRegistration>());

        Assert.True(result.ContainsKey(500));
        Assert.Single(result[500]);
        Assert.Equal(typeof(FakeProvider), result[500][0].ProviderType);
    }

    [Fact]
    public void Collect_MethodAndClassAttributes_DifferentStatuses_BothApply()
    {
        var method = MethodOf(nameof(ClassWithExample.MethodWithDifferentStatus));
        var result = ResponseExampleResolver.Collect(method, Array.Empty<GlobalExampleRegistration>());

        Assert.Contains(404, result.Keys);
        Assert.Contains(500, result.Keys);
        Assert.Equal(typeof(AnotherProvider), result[404][0].ProviderType);
        Assert.Equal(typeof(FakeProvider), result[500][0].ProviderType);
    }

    [Fact]
    public void Collect_MethodOverridesClassForSameStatus()
    {
        var method = MethodOf(nameof(ClassWithExample.MethodOverridesClassForSameStatus));
        var result = ResponseExampleResolver.Collect(method, Array.Empty<GlobalExampleRegistration>());

        Assert.Single(result[500]);
        Assert.Equal(typeof(AnotherProvider), result[500][0].ProviderType);
    }

    [Fact]
    public void Collect_GlobalIsIgnoredWhenMethodCoversSameStatus()
    {
        var method = MethodOf(nameof(ClassWithExample.MethodOverridesClassForSameStatus));
        var globals = new[]
        {
            new GlobalExampleRegistration(500, typeof(FakeProvider), "global")
        };
        var result = ResponseExampleResolver.Collect(method, globals);

        Assert.Single(result[500]);
        Assert.Equal(typeof(AnotherProvider), result[500][0].ProviderType); // method wins, global dropped
    }

    [Fact]
    public void Collect_GlobalAppliesWhenNeitherMethodNorClassCoversStatus()
    {
        var method = MethodOf(nameof(ClassWithoutExample.MultiNamedSameStatus));
        var globals = new[]
        {
            new GlobalExampleRegistration(500, typeof(FakeProvider))
        };
        var result = ResponseExampleResolver.Collect(method, globals);

        Assert.True(result.ContainsKey(500));
        Assert.Equal(typeof(FakeProvider), result[500][0].ProviderType);
    }

    [Fact]
    public void Collect_GlobalIsIgnoredWhenClassCoversSameStatus()
    {
        var method = MethodOf(nameof(ClassWithExample.MethodWithDifferentStatus));
        var globals = new[]
        {
            new GlobalExampleRegistration(500, typeof(AnotherProvider))
        };
        var result = ResponseExampleResolver.Collect(method, globals);

        // class declares 500, global must NOT add another 500 entry
        Assert.Single(result[500]);
        Assert.Equal(typeof(FakeProvider), result[500][0].ProviderType);
    }

    [Fact]
    public void Collect_MultipleNamedExamplesForSameStatus_BothKept()
    {
        var method = MethodOf(nameof(ClassWithoutExample.MultiNamedSameStatus));
        var result = ResponseExampleResolver.Collect(method, Array.Empty<GlobalExampleRegistration>());

        Assert.Equal(2, result[400].Count);
        Assert.Contains(result[400], r => r.Name == "default");
        Assert.Contains(result[400], r => r.Name == "edge");
    }

    [Fact]
    public void CreateProvider_ResolvesFromDi_WhenRegistered()
    {
        // Build a tiny IServiceProvider that returns a specific instance
        var preBuilt = new FakeProvider();
        var services = new FakeServiceProvider(typeof(FakeProvider), preBuilt);

        var resolved = ResponseExampleResolver.CreateProvider(typeof(FakeProvider), services);

        Assert.Same(preBuilt, resolved);
    }

    [Fact]
    public void CreateProvider_FallsBackToActivator_WhenNotInDi()
    {
        var services = new FakeServiceProvider(typeof(FakeProvider), null); // returns null
        var resolved = ResponseExampleResolver.CreateProvider(typeof(FakeProvider), services);

        Assert.NotNull(resolved);
        Assert.IsType<FakeProvider>(resolved);
    }

    [Fact]
    public void CreateProvider_ReturnsNull_WhenActivatorCannotConstruct()
    {
        // Provider needs ctor argument that Activator can't supply
        var resolved = Record.Exception(() =>
            ResponseExampleResolver.CreateProvider(typeof(ProviderWithCtorDependency), services: null));

        // Activator.CreateInstance throws MissingMethodException for unsatisfiable ctors;
        // ensure callers see a meaningful failure rather than silent null.
        Assert.NotNull(resolved);
    }

    [Fact]
    public void InvokeGetExample_ReturnsProviderOutput()
    {
        var provider = new FakeProvider();
        var result = ResponseExampleResolver.InvokeGetExample(provider);
        Assert.NotNull(result);
    }

    [Fact]
    public void InvokeGetExample_NonProvider_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ResponseExampleResolver.InvokeGetExample(new object()));
    }

    [Fact]
    public void Serialize_NullInput_ReturnsNull()
    {
        var node = ResponseExampleResolver.Serialize(null, options: null);
        Assert.Null(node);
    }

    [Fact]
    public void Serialize_Object_ProducesJsonNode()
    {
        var node = ResponseExampleResolver.Serialize(new { value = 42 }, options: null);
        Assert.NotNull(node);
        Assert.Equal(42, node!["value"]!.GetValue<int>());
    }

    private sealed class FakeServiceProvider : IServiceProvider
    {
        private readonly Type _key;
        private readonly object? _value;
        public FakeServiceProvider(Type key, object? value) { _key = key; _value = value; }
        public object? GetService(Type serviceType) => serviceType == _key ? _value : null;
    }
}
