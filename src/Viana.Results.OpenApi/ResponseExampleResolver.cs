using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Viana.Results.OpenApi;

/// <summary>
/// Internal helper that materializes <see cref="ResponseExampleAttribute"/> declarations
/// (method, class, and global) into JSON examples ready to be plugged into an OpenAPI document.
/// Resolution order (most specific wins): method &gt; class &gt; global.
/// </summary>
public static class ResponseExampleResolver
{
    /// <summary>
    /// Collects every example that applies to <paramref name="method"/>, grouped by status code.
    /// When a status is covered by a method-level attribute, class and global examples for the
    /// same status are excluded.
    /// </summary>
    /// <param name="method">The action method whose attributes drive the lookup.</param>
    /// <param name="globalExamples">Globally-registered examples (from <see cref="VianaResultOptions"/>).</param>
    /// <returns>Map of status code to the list of examples that should appear under that status.</returns>
    public static Dictionary<int, List<ResolvedExample>> Collect(MethodInfo method, IReadOnlyList<GlobalExampleRegistration> globalExamples)
    {
        var byStatus = new Dictionary<int, List<ResolvedExample>>();

        var methodAttrs = method.GetCustomAttributes<ResponseExampleAttribute>(true).ToList();
        var classAttrs = method.DeclaringType?.GetCustomAttributes<ResponseExampleAttribute>(true).ToList()
                         ?? new List<ResponseExampleAttribute>();

        var methodStatuses = methodAttrs.Select(a => a.StatusCode).ToHashSet();

        foreach (var attr in methodAttrs)
            Add(byStatus, new ResolvedExample(attr.StatusCode, attr.ProviderType, attr.Name, attr.Summary, attr.Description));

        foreach (var attr in classAttrs)
        {
            if (methodStatuses.Contains(attr.StatusCode))
                continue;
            Add(byStatus, new ResolvedExample(attr.StatusCode, attr.ProviderType, attr.Name, attr.Summary, attr.Description));
        }

        var classStatuses = classAttrs.Select(a => a.StatusCode).ToHashSet();
        foreach (var g in globalExamples)
        {
            if (methodStatuses.Contains(g.StatusCode) || classStatuses.Contains(g.StatusCode))
                continue;
            Add(byStatus, new ResolvedExample(g.StatusCode, g.ProviderType, g.Name, g.Summary, g.Description));
        }

        return byStatus;
    }

    private static void Add(Dictionary<int, List<ResolvedExample>> map, ResolvedExample value)
    {
        if (!map.TryGetValue(value.StatusCode, out var list))
        {
            list = new List<ResolvedExample>();
            map[value.StatusCode] = list;
        }
        list.Add(value);
    }

    /// <summary>
    /// Instantiates an <see cref="IExampleProvider{T}"/> by trying DI first, then falling back
    /// to <see cref="Activator.CreateInstance(Type)"/>.
    /// </summary>
    public static object? CreateProvider(Type providerType, IServiceProvider? services)
    {
        if (services != null)
        {
            var fromDi = services.GetService(providerType);
            if (fromDi != null)
                return fromDi;
        }

        return Activator.CreateInstance(providerType);
    }

    /// <summary>
    /// Calls <c>GetExample()</c> on the provider instance via reflection (the closed generic
    /// interface is unknown at compile time on the consumer side).
    /// </summary>
    public static object? InvokeGetExample(object provider)
    {
        var providerType = provider.GetType();
        var iface = providerType.GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExampleProvider<>));

        if (iface == null)
            throw new InvalidOperationException(
                $"Type '{providerType.FullName}' does not implement IExampleProvider<T>.");

        var method = iface.GetMethod(nameof(IExampleProvider<object>.GetExample))
            ?? throw new InvalidOperationException("IExampleProvider<T>.GetExample method missing.");

        return method.Invoke(provider, Array.Empty<object>());
    }

    /// <summary>
    /// Serializes the example object using the application's JSON options and returns the
    /// result as a <see cref="JsonNode"/> suitable for plugging into an OpenAPI media type.
    /// </summary>
    public static JsonNode? Serialize(object? example, JsonSerializerOptions? options)
    {
        if (example is null)
            return null;

        return JsonSerializer.SerializeToNode(example, example.GetType(), options);
    }
}

/// <summary>A flat, resolved example entry (after merging method/class/global precedence).</summary>
public sealed record ResolvedExample(int StatusCode, Type ProviderType, string? Name, string? Summary, string? Description);
