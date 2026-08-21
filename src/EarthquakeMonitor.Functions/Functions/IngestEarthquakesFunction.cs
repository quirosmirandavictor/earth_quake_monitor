using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;
using EarthquakeMonitor.Earthquakes.Application.UseCases.Ingestion;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EarthquakeMonitor.Functions.Functions;

public sealed class IngestEarthquakesFunction(
    IIngestEarthquakesUseCase useCase,
    IConfiguration configuration,
    ILogger<IngestEarthquakesFunction> logger)
{
    [Function("IngestEarthquakes")]
    public async Task Run(
        [TimerTrigger("%EarthquakeIngestionSchedule%")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        var end = DateTimeOffset.UtcNow;
        var overlapMinutes = configuration.GetValue<int?>("USGS:OverlapMinutes")
            ?? throw new InvalidOperationException("USGS:OverlapMinutes is not configured.");
        var lookbackHours = configuration.GetValue<int?>("USGS:InitialLookbackHours")
            ?? throw new InvalidOperationException("USGS:InitialLookbackHours is not configured.");
        var start = end.AddHours(-lookbackHours).AddMinutes(-overlapMinutes);

        logger.LogInformation("Starting USGS ingestion for {Start} to {End}.", start, end);

        var result = await useCase.ExecuteAsync(new SourceQuery(
            StartTime: start,
            EndTime: end,
            UpdatedAfter: start,
            Limit: configuration.GetValue<int?>("USGS:PageSize")
                ?? throw new InvalidOperationException("USGS:PageSize is not configured.")), cancellationToken);

        if (result.FailedCount > 0)
        {
            throw new InvalidOperationException(
                $"USGS ingestion completed with {result.FailedCount} persistence failures " +
                $"out of {result.ReceivedCount} received events.");
        }
    }
}
