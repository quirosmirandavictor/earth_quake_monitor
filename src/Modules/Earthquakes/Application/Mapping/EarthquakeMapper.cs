using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;
using EarthquakeMonitor.Earthquakes.Domain;

namespace EarthquakeMonitor.Earthquakes.Application.Mapping;

public sealed class EarthquakeMapper
{
    public Earthquake Map(ExternalEarthquake external) => Earthquake.Create(
        source: external.Source,
        externalId: external.ExternalId,
        originTime: external.OriginTime,
        latitude: external.Latitude,
        longitude: external.Longitude,
        depthKm: external.DepthKm,
        magnitude: external.Magnitude,
        magnitudeType: external.MagnitudeType,
        place: external.Place,
        eventUrl: external.EventUrl,
        providerUpdatedAt: external.ProviderUpdatedAt,
        providerStatus: external.ProviderStatus,
        tsunami: external.Tsunami,
        alertLevel: external.AlertLevel,
        significance: external.Significance,
        rawPayload: external.RawPayload);
}
