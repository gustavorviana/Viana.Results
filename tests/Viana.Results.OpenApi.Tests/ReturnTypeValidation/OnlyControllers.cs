using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Registers exactly the given (internal) controllers, so they never leak into other test hosts.</summary>
public sealed class OnlyControllers(params Type[] controllers) : IApplicationFeatureProvider<ControllerFeature>
{
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        foreach (var controller in controllers)
            feature.Controllers.Add(controller.GetTypeInfo());
    }
}
