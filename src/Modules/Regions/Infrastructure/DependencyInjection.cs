using EarthquakeMonitor.Regions.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EarthquakeMonitor.Regions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRegionsInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRegionCatalog, RegionCatalog>();
        return services;
    }
}
