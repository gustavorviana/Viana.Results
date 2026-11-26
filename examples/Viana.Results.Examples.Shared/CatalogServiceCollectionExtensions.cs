using Microsoft.Extensions.DependencyInjection;
using Viana.Results.Examples.Shared.Catalog;

namespace Viana.Results.Examples.Shared;

/// <summary>Registers the in-memory catalog shared by every example host.</summary>
public static class CatalogServiceCollectionExtensions
{
    /// <summary>Adds <see cref="ProductCatalog"/> and <see cref="InventoryGateway"/> as singletons.</summary>
    public static IServiceCollection AddCatalog(this IServiceCollection services)
    {
        services.AddSingleton<ProductCatalog>();
        services.AddSingleton<InventoryGateway>();
        return services;
    }
}
