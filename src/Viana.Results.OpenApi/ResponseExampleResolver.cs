using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Viana.Results.OpenApi;

/// <summary>Resolves <see cref="ResponseExampleAttribute"/> and global example registrations.</summary>
[Obsolete("Internal helper; not meant for direct use.")]
public static class ResponseExampleResolver
{
    /// <summary>Collects examples by status (method &gt; class &gt; global).</summary>
    public static Dictionary<int, List<ResolvedExample>> Collect(MethodInfo method, IReadOnlyList<GlobalExampleRegistration> globalExamples)
        => ExampleResolver.Collect(method, globalExamples).ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Select(e => new ResolvedExample(e.StatusCode, e.ProviderType, e.Name, e.Summary, e.Description)).ToList());

    /// <summary>Creates an example provider (DI first, then constructor injection, then parameterless).</summary>
    public static object? CreateProvider(Type providerType, IServiceProvider? services) => ExampleResolver.CreateProvider(providerType, services);

    /// <summary>Calls <c>GetExample()</c> on the provider.</summary>
    public static object? InvokeGetExample(object provider) => ExampleResolver.InvokeGetExample(provider);

    /// <summary>Serializes the example with the application's JSON options.</summary>
    public static JsonNode? Serialize(object? example, JsonSerializerOptions? options) => ExampleResolver.Serialize(example, options);
}

/// <summary>A resolved example registration.</summary>
[Obsolete("Internal helper; not meant for direct use.")]
public sealed record ResolvedExample(int StatusCode, Type ProviderType, string? Name, string? Summary, string? Description);
