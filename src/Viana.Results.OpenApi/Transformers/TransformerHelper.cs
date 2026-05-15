using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Linq;
using System.Reflection;

namespace Viana.Results.OpenApi.Transformers;

internal static class TransformerHelper
{
    /// <summary>
    /// Extracts the MethodInfo from an ApiDescription.
    /// Works for both MVC controllers (via ControllerActionDescriptor) and Minimal APIs (via endpoint metadata).
    /// </summary>
    internal static MethodInfo? GetMethodInfo(ApiDescription description)
    {
        if (description.ActionDescriptor is ControllerActionDescriptor controllerDescriptor)
            return controllerDescriptor.MethodInfo;

        var methodInfoMetadata = description.ActionDescriptor?.EndpointMetadata
            ?.OfType<MethodInfo>()
            .FirstOrDefault();

        return methodInfoMetadata;
    }
}
