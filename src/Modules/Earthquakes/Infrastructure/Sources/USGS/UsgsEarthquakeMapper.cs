using System.Text.Json;
using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;
using EarthquakeMonitor.Earthquakes.Infrastructure.Sources.USGS.Models;

namespace EarthquakeMonitor.Earthquakes.Infrastructure.Sources.USGS;

public sealed class UsgsEarthquakeMapper
{
    public ExternalEarthquake Map(UsgsFeature feature)
    {
        if (string.IsNullOrWhiteSpace(feature.Id))
            throw new InvalidDataException("USGS feature id is required.");
        if (feature.Geometry?.Coordinates is not { Length: >= 2 } coordinates)
            throw new InvalidDataException($"USGS feature '{feature.Id}' has invalid coordinates.");
        if (feature.Geometry.Type is not null && !string.Equals(feature.Geometry.Type, "Point", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"USGS feature '{feature.Id}' is not a Point geometry.");
        if (!feature.Properties.TimeUnixMilliseconds.HasValue)
            throw new InvalidDataException($"USGS feature '{feature.Id}' has no origin time.");

        var longitude = coordinates[0];
        var latitude = coordinates[1];
        if (latitude is < -90 or > 90) throw new InvalidDataException($"Invalid latitude for '{feature.Id}'.");
        if (longitude is < -180 or > 180) throw new InvalidDataException($"Invalid longitude for '{feature.Id}'.");

        return new ExternalEarthquake(
            "USGS", feature.Id,
            DateTimeOffset.FromUnixTimeMilliseconds(feature.Properties.TimeUnixMilliseconds.Value),
            latitude, longitude, coordinates.Length > 2 ? coordinates[2] : null,
            feature.Properties.Magnitude, feature.Properties.MagnitudeType, feature.Properties.Place,
            feature.Properties.Url,
            feature.Properties.UpdatedUnixMilliseconds is long updated
                ? DateTimeOffset.FromUnixTimeMilliseconds(updated) : null,
            feature.Properties.Status, feature.Properties.Tsunami, feature.Properties.Alert,
            feature.Properties.Significance, JsonSerializer.Serialize(feature));
    }
}
