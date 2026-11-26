using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Viana.Results.OpenApi.Processing;

/// <summary>Where an operation comes from: its controller (null for minimal APIs) and the warning logger.</summary>
internal sealed record EndpointScope(Type? Controller, ILogger? Logger)
{
    internal const string LoggerCategory = "Viana.Results.OpenApi";

    internal static EndpointScope From(object? actionDescriptor, IServiceProvider? services)
    {
        var controller = (actionDescriptor as ControllerActionDescriptor)?.ControllerTypeInfo.AsType();
        var logger = (services?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(LoggerCategory);
        return new EndpointScope(controller, logger);
    }
}

/// <summary>
/// Feature 013: Viana response declarations only make sense on endpoints that return a Viana result
/// (directly or wrapped in <see cref="Task{T}"/>, <see cref="ValueTask{T}"/> or <see cref="ActionResult{T}"/>).
/// </summary>
internal static class ReturnTypeRules
{
    private static readonly ConcurrentDictionary<Type, bool> ControllerCache = new();

    /// <summary>The Viana result type carried by <paramref name="type"/>, or null when it is not a result return.</summary>
    internal static Type? ResultTypeOf(Type type)
    {
        if (typeof(IResult).IsAssignableFrom(type))
            return type;

        if (!type.IsGenericType)
            return null;

        var definition = type.GetGenericTypeDefinition();
        if (definition == typeof(Task<>) || definition == typeof(ValueTask<>) || definition == typeof(ActionResult<>))
            return ResultTypeOf(type.GetGenericArguments()[0]);

        return null;
    }

    internal static bool IsResultReturn(Type type) => ResultTypeOf(type) != null;

    /// <summary>Whether at least one action of <paramref name="controller"/> returns a Viana result (cached).</summary>
    internal static bool ControllerHasResultAction(Type controller)
        => ControllerCache.GetOrAdd(controller, static type => type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName
                && m.DeclaringType != typeof(object)
                && m.DeclaringType != typeof(ControllerBase)
                && m.DeclaringType != typeof(Controller)
                && m.GetCustomAttribute<NonActionAttribute>(true) == null)
            .Any(m => IsResultReturn(m.ReturnType)));

    /// <summary>
    /// Decides whether the declarations of <paramref name="attributeType"/> apply to the operation.
    /// Method-level on a non-result: error. Class-level on a non-result: error when the controller has no result
    /// action, otherwise skipped with a warning. A result return always applies.
    /// </summary>
    internal static bool Applies(MethodInfo method, Type? owner, Type attributeType, EndpointScope? scope)
    {
        if (IsResultReturn(method.ReturnType))
            return true;

        var name = AttributeName(attributeType);
        if (method.IsDefined(attributeType, true))
            throw new InvalidOperationException(ActionMessage(owner ?? method.DeclaringType, method, name));

        if (owner == null || !owner.IsDefined(attributeType, true))
            return false;

        if (!ControllerHasResultAction(owner))
            throw new InvalidOperationException(ControllerMessage(owner, name));

        scope?.Logger?.LogWarning(
            "{Controller}.{Action}: class-level [{Attribute}] skipped, the action does not return a Viana result.",
            owner.Name, method.Name, name);
        return false;
    }

    internal static string AttributeName(Type attributeType)
        => attributeType.Name.EndsWith("Attribute", StringComparison.Ordinal)
            ? attributeType.Name.Substring(0, attributeType.Name.Length - "Attribute".Length)
            : attributeType.Name;

    internal static string MinimalMessage(string endpointName, string declaration, Type returnType)
        => $"Endpoint '{endpointName}': {declaration} requires the endpoint to return a Viana result, but it returns {TypeName(returnType)}.";

    internal static string ActionMessage(Type? controller, MethodInfo method, string attribute)
        => $"Action '{controller?.Name}.{method.Name}': [{attribute}] requires the action to return a Viana result, but it returns {TypeName(method.ReturnType)}.";

    internal static string ControllerMessage(Type controller, string attribute)
        => $"Controller '{controller.Name}': [{attribute}] is declared on the class but no action returns a Viana result.";

    private static string TypeName(Type type)
    {
        if (!type.IsGenericType)
            return type == typeof(Microsoft.AspNetCore.Http.IResult) ? type.FullName! : type.Name;

        var name = type.Name.Substring(0, type.Name.IndexOf('`'));
        return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(TypeName))}>";
    }
}
