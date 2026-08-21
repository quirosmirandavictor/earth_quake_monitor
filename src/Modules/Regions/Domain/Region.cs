namespace EarthquakeMonitor.Regions.Domain;

public enum RegionCode
{
    CostaRica,
    CentralAmerica,
    Caribbean,
    Global
}

public sealed record GeographicBounds(
    decimal MinimumLatitude,
    decimal MaximumLatitude,
    decimal MinimumLongitude,
    decimal MaximumLongitude)
{
    public bool Contains(decimal latitude, decimal longitude) =>
        latitude >= MinimumLatitude && latitude <= MaximumLatitude &&
        longitude >= MinimumLongitude && longitude <= MaximumLongitude;
}

public sealed record RegionDefinition(RegionCode Code, string Name, GeographicBounds? Bounds);
