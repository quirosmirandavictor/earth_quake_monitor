namespace EarthquakeMonitor.Analytics.Domain;

public sealed record EarthquakeSummary(
    DateTimeOffset From,
    DateTimeOffset To,
    int EventCount,
    decimal? MaximumMagnitude,
    decimal? AverageMagnitude);
