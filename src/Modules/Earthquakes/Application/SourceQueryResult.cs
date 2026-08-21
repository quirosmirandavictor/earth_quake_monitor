namespace EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;

public sealed record SourceQueryResult(
    IReadOnlyList<ExternalEarthquake> Events,
    bool HasMoreResults,
    string? NextCursor);

public sealed record ExternalEarthquake(
    string Source, string ExternalId, DateTimeOffset OriginTime,
    decimal Latitude, decimal Longitude, decimal? DepthKm, decimal? Magnitude,
    string? MagnitudeType, string? Place, string? EventUrl,
    DateTimeOffset? ProviderUpdatedAt, string? ProviderStatus, int? Tsunami,
    string? AlertLevel, int? Significance, string RawPayload);
