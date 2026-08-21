using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;

public interface ISeismicSource
{
    Task<SourceQueryResult> GetEventsAsync(
        SourceQuery query,
        CancellationToken cancellationToken = default);
}