using EarthquakeMonitor.Analytics.Domain;

namespace EarthquakeMonitor.Analytics.Application.Abstractions;

public interface IAnalyticsRepository
{
    Task<EarthquakeSummary> GetSummaryAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}
