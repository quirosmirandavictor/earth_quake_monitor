using EarthquakeMonitor.Earthquakes.Application.Abstractions.Persistence;
using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;
using EarthquakeMonitor.Earthquakes.Application.Mapping;
using Microsoft.Extensions.Logging;

namespace EarthquakeMonitor.Earthquakes.Application.UseCases.Ingestion;

public sealed class IngestEarthquakesUseCase(
    ISeismicSource source,
    IEarthquakeRepository repository,
    EarthquakeMapper mapper,
    ILogger<IngestEarthquakesUseCase> logger) : IIngestEarthquakesUseCase
{
    public async Task<IngestionResult> ExecuteAsync(
        SourceQuery query,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var sourceResult = await source.GetEventsAsync(query, cancellationToken);
        var persistedCount = 0;
        var failedCount = 0;

        foreach (var externalEvent in sourceResult.Events)
        {
            try
            {
                await repository.UpsertAsync(mapper.Map(externalEvent), cancellationToken);
                persistedCount++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                failedCount++;
                logger.LogError(exception, "Failed to persist earthquake {Source}/{ExternalId}.",
                    externalEvent.Source, externalEvent.ExternalId);
            }
        }

        var finishedAt = DateTimeOffset.UtcNow;
        logger.LogInformation("Earthquake ingestion completed. Received={Received}, Persisted={Persisted}, Failed={Failed}.",
            sourceResult.Events.Count, persistedCount, failedCount);

        return new IngestionResult(startedAt, finishedAt, sourceResult.Events.Count, persistedCount, failedCount);
    }
}
