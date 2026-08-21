using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;

namespace EarthquakeMonitor.Earthquakes.Application.UseCases.Ingestion;

public interface IIngestEarthquakesUseCase
{
    Task<IngestionResult> ExecuteAsync(
        SourceQuery query,
        CancellationToken cancellationToken = default);
}

public sealed record IngestionResult(
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    int ReceivedCount,
    int PersistedCount,
    int FailedCount);
