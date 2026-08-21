namespace EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;

public sealed record SourceQuery(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    DateTimeOffset? UpdatedAfter = null,
    decimal? MinimumMagnitude = null,
    decimal? MinimumLatitude = null,
    decimal? MaximumLatitude = null,
    decimal? MinimumLongitude = null,
    decimal? MaximumLongitude = null,
    int Offset = 1,
    int Limit = 20000);
