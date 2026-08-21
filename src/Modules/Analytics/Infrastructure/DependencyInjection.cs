using EarthquakeMonitor.Analytics.Application.Abstractions;
using EarthquakeMonitor.Analytics.Infrastructure.Persistence.Oracle;
using Microsoft.Extensions.DependencyInjection;

namespace EarthquakeMonitor.Analytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAnalyticsRepository, OracleAnalyticsRepository>();
        return services;
    }
}
