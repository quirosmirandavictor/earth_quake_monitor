using EarthquakeMonitor.Earthquakes.Domain;

namespace EarthquakeMonitor.Earthquakes.Application.Abstractions.Persistence;

public interface IEarthquakeRepository
{
    Task UpsertAsync(Earthquake earthquake, CancellationToken cancellationToken = default);
    Task<Earthquake?> GetBySourceIdentityAsync(string source, string externalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Earthquake>> SearchAsync(EarthquakeQuery query, CancellationToken cancellationToken = default);
}

public sealed record EarthquakeQuery(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    decimal? MinimumMagnitude = null,
    decimal? MaximumMagnitude = null,
    decimal? MinimumLatitude = null,
    decimal? MaximumLatitude = null,
    decimal? MinimumLongitude = null,
    decimal? MaximumLongitude = null,
    int Limit = 100);
