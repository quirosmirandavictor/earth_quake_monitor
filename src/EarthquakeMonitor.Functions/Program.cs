using Azure.Monitor.OpenTelemetry.Exporter;
using EarthquakeMonitor.Earthquakes.Infrastructure;
using EarthquakeMonitor.Regions.Infrastructure;
using EarthquakeMonitor.Analytics.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("configuration/regions.json", optional: false, reloadOnChange: false);
builder.ConfigureFunctionsWebApplication();
builder.Services.AddEarthquakesInfrastructure();
builder.Services.AddRegionsInfrastructure();
builder.Services.AddAnalyticsInfrastructure();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
