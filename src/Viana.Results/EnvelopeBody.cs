using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace Viana.Results;

/// <summary>
/// Feature 014: builds the JSON body of an <see cref="IEnvelopeResult{T}"/> from the serializer contract of the
/// application options (naming policy, converters, <c>[JsonPropertyName]</c>, <c>[JsonIgnore]</c>), without
/// <see cref="IResult.Status"/>, <see cref="IResult.Problem"/> and the <see cref="IHasExtensions.Extensions"/>
/// container. Extension entries are written at the root; declared members win on a name clash.
/// </summary>
/// <remarks>Only <see cref="JsonTypeInfo"/> based APIs are used, so no reflection-only serializer path is required.</remarks>
internal static class EnvelopeBody
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> WithResolver = new();

    // Per options and type: whether the contract can write Status/Problem/Extensions, i.e. needs a shaped node.
    // Keyed by options (not JsonTypeInfo): mutable options hand out a new JsonTypeInfo on every GetTypeInfo call,
    // so the contract is only resolved when the decision is unknown or the body must be shaped.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<JsonSerializerOptions, System.Collections.Concurrent.ConcurrentDictionary<Type, bool>> NeedsShaping = new();

    /// <summary>
    /// The body to write: the instance itself when its contract already hides the container members
    /// (e.g. <see cref="PagedResult{T}"/>, whose Status/Problem are ignored), otherwise a shaped JSON object.
    /// </summary>
    internal static object Shape(IResult result, JsonSerializerOptions options)
    {
        options = EnsureResolver(options);
        var type = result.GetType();
        var decisions = NeedsShaping.GetValue(options, static _ => new System.Collections.Concurrent.ConcurrentDictionary<Type, bool>());
        if (!decisions.TryGetValue(type, out var needs))
        {
            needs = ComputeNeedsShaping(options.GetTypeInfo(type), result);
            decisions[type] = needs;
        }

        return needs ? Build(result, options.GetTypeInfo(type), options) : result;
    }

    // Feature 015: decided once per contract from the contract itself, never from one instance's output.
    // A container member is never written only when it has no getter or an unconditional [JsonIgnore];
    // any conditional rule (or a global option the contract does not show) counts as may be written: shape.
    private static bool ComputeNeedsShaping(JsonTypeInfo info, IResult result)
    {
        if (result is IHasExtensions)
            return true;

        foreach (var property in info.Properties)
        {
            if (IsContainerMember(property, result) && !IsNeverWritten(property))
                return true;
        }
        return false;
    }

    private static bool IsNeverWritten(JsonPropertyInfo property)
    {
        if (property.Get == null)
            return true;

        return property.AttributeProvider is PropertyInfo clr
            && clr.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>(true) is { Condition: System.Text.Json.Serialization.JsonIgnoreCondition.Always };
    }

    internal static JsonObject Build(IResult result, JsonSerializerOptions options)
    {
        options = EnsureResolver(options);
        return Build(result, options.GetTypeInfo(result.GetType()), options);
    }

    private static JsonObject Build(IResult result, JsonTypeInfo info, JsonSerializerOptions options)
    {
        var body = JsonSerializer.SerializeToNode(result, info) as JsonObject ?? new JsonObject();

        foreach (var property in info.Properties)
        {
            if (IsContainerMember(property, result))
                body.Remove(property.Name);
        }

        if (result is IHasExtensions withExtensions)
        {
            var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var member in body)
                taken.Add(member.Key);

            foreach (var entry in withExtensions.Extensions)
            {
                if (!taken.Add(entry.Key))
                    continue;

                body[entry.Key] = entry.Value == null
                    ? null
                    : JsonSerializer.SerializeToNode(entry.Value, options.GetTypeInfo(entry.Value.GetType()));
            }
        }

        return body;
    }

    // Options created without a resolver (new JsonSerializerOptions()) get the reflection resolver, as
    // JsonSerializer would, on a cached copy so the caller's options are neither changed nor locked.
    // Applications with reflection disabled (trimmed/AOT) must configure a resolver, as for any serialization.
    private static JsonSerializerOptions EnsureResolver(JsonSerializerOptions options)
    {
        if (options.TypeInfoResolver != null || !JsonSerializer.IsReflectionEnabledByDefault)
            return options;

        return WithResolver.GetValue(options, CreateReflectionCopy);
    }

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "Only reached when System.Text.Json reflection is enabled (IsReflectionEnabledByDefault).")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050",
        Justification = "Only reached when System.Text.Json reflection is enabled (IsReflectionEnabledByDefault).")]
#endif
    private static JsonSerializerOptions CreateReflectionCopy(JsonSerializerOptions options)
        => new(options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    // Matched by the CLR property behind the contract member, so renamed members are found too.
    private static bool IsContainerMember(JsonPropertyInfo property, IResult result)
    {
        if (property.AttributeProvider is not PropertyInfo clr)
            return false;

        return clr.Name switch
        {
            nameof(IResult.Status) => clr.PropertyType == typeof(int),
            nameof(IResult.Problem) => clr.PropertyType == typeof(ProblemResult),
            nameof(IHasExtensions.Extensions) => result is IHasExtensions,
            _ => false,
        };
    }
}
