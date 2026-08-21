using EarthquakeMonitor.Earthquakes.Application.Abstractions.Persistence;
using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;
using EarthquakeMonitor.Earthquakes.Application.Mapping;
using EarthquakeMonitor.Earthquakes.Application.UseCases.Ingestion;
using EarthquakeMonitor.Earthquakes.Infrastructure.Persistence.Oracle;
using EarthquakeMonitor.Earthquakes.Infrastructure.Sources.USGS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EarthquakeMonitor.Earthquakes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEarthquakesInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<OracleConnectionFactory>();
        services.AddScoped<IEarthquakeRepository, OracleEarthquakeRepository>();
        services.AddSingleton<UsgsEarthquakeMapper>();
        services.AddSingleton<EarthquakeMapper>();
        services.AddScoped<IIngestEarthquakesUseCase, IngestEarthquakesUseCase>();
        services.AddHttpClient<ISeismicSource, UsgsEarthquakeSource>((serviceProvider, client) =>
        {
            var configuration = serviceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            client.BaseAddress = new Uri(configuration["USGS:BaseUrl"] ?? "https://earthquake.usgs.gov/");
            client.Timeout = TimeSpan.FromSeconds(configuration.GetValue("USGS:TimeoutSeconds", 30));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EarthquakeMonitor/1.0");
        });
        return services;
    }
}
