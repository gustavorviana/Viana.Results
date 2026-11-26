using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Viana.Results.OpenApi.Tests.ReturnTypeValidation;

/// <summary>Collects the endpoint metadata that the minimal API extensions add.</summary>
public sealed class StubEndpointBuilder : IEndpointConventionBuilder
{
    public List<object> Metadata { get; } = new();

    public void Add(Action<EndpointBuilder> convention)
    {
        var endpoint = new RouteEndpointBuilder(_ => Task.CompletedTask, RoutePatternFactory.Parse("/x"), 0);
        convention(endpoint);
        Metadata.AddRange(endpoint.Metadata);
    }

    public static List<object> With(Action<StubEndpointBuilder> declare)
    {
        var builder = new StubEndpointBuilder();
        declare(builder);
        return builder.Metadata;
    }
}
