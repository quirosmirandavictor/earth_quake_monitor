namespace EarthquakeMonitor.Earthquakes.Domain;

public sealed class Earthquake
{
    private Earthquake() { }

    public Guid Id { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public string ExternalId { get; private set; } = string.Empty;
    public DateTimeOffset OriginTime { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public decimal? DepthKm { get; private set; }
    public decimal? Magnitude { get; private set; }
    public string? MagnitudeType { get; private set; }
    public string? Place { get; private set; }
    public string? EventUrl { get; private set; }
    public DateTimeOffset? ProviderUpdatedAt { get; private set; }
    public string? ProviderStatus { get; private set; }
    public int? Tsunami { get; private set; }
    public string? AlertLevel { get; private set; }
    public int? Significance { get; private set; }
    public string? RawPayload { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Earthquake Create(string source, string externalId, DateTimeOffset originTime,
        decimal latitude, decimal longitude, decimal? depthKm = null, decimal? magnitude = null,
        string? magnitudeType = null, string? place = null, string? eventUrl = null,
        DateTimeOffset? providerUpdatedAt = null, string? providerStatus = null,
        int? tsunami = null, string? alertLevel = null, int? significance = null,
        string? rawPayload = null, DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Source is required.", nameof(source));
        if (string.IsNullOrWhiteSpace(externalId)) throw new ArgumentException("External ID is required.", nameof(externalId));
        if (latitude is < -90 or > 90) throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude is < -180 or > 180) throw new ArgumentOutOfRangeException(nameof(longitude));
        var timestamp = now ?? DateTimeOffset.UtcNow;
        return new Earthquake { Id = Guid.NewGuid(), Source = source.Trim(), ExternalId = externalId.Trim(), OriginTime = originTime,
            Latitude = latitude, Longitude = longitude, DepthKm = depthKm, Magnitude = magnitude,
            MagnitudeType = magnitudeType, Place = place, EventUrl = eventUrl,
            ProviderUpdatedAt = providerUpdatedAt, ProviderStatus = providerStatus,
            Tsunami = tsunami, AlertLevel = alertLevel, Significance = significance,
            RawPayload = rawPayload, CreatedAt = timestamp, UpdatedAt = timestamp };
    }

    public static Earthquake FromPersistence(Guid id, string source, string externalId,
        DateTimeOffset originTime, decimal latitude, decimal longitude, decimal? depthKm,
        decimal? magnitude, string? magnitudeType, string? place, string? eventUrl,
        DateTimeOffset? providerUpdatedAt, string? providerStatus, int? tsunami,
        string? alertLevel, int? significance, string? rawPayload,
        DateTimeOffset createdAt, DateTimeOffset updatedAt) => new()
    {
        Id = id, Source = source, ExternalId = externalId, OriginTime = originTime,
        Latitude = latitude, Longitude = longitude, DepthKm = depthKm, Magnitude = magnitude,
        MagnitudeType = magnitudeType, Place = place, EventUrl = eventUrl,
        ProviderUpdatedAt = providerUpdatedAt, ProviderStatus = providerStatus,
        Tsunami = tsunami, AlertLevel = alertLevel, Significance = significance,
        RawPayload = rawPayload,
        CreatedAt = createdAt, UpdatedAt = updatedAt
    };
}
