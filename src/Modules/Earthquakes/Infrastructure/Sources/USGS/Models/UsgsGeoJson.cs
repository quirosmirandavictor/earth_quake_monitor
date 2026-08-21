using System.Text.Json.Serialization;

namespace EarthquakeMonitor.Earthquakes.Infrastructure.Sources.USGS.Models;

public sealed class UsgsFeatureCollection
{
    [JsonPropertyName("features")]
    public List<UsgsFeature> Features { get; init; } = [];
}

public sealed class UsgsFeature
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("properties")]
    public UsgsProperties Properties { get; init; } = new();

    [JsonPropertyName("geometry")]
    public UsgsGeometry? Geometry { get; init; }
}

public sealed class UsgsProperties
{
    [JsonPropertyName("mag")] public decimal? Magnitude { get; init; }
    [JsonPropertyName("place")] public string? Place { get; init; }
    [JsonPropertyName("time")] public long? TimeUnixMilliseconds { get; init; }
    [JsonPropertyName("updated")] public long? UpdatedUnixMilliseconds { get; init; }
    [JsonPropertyName("url")] public string? Url { get; init; }
    [JsonPropertyName("status")] public string? Status { get; init; }
    [JsonPropertyName("tsunami")] public int? Tsunami { get; init; }
    [JsonPropertyName("alert")] public string? Alert { get; init; }
    [JsonPropertyName("sig")] public int? Significance { get; init; }
    [JsonPropertyName("magType")] public string? MagnitudeType { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
}

public sealed class UsgsGeometry
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("coordinates")] public decimal[]? Coordinates { get; init; }
}
