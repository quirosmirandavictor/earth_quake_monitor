using EarthquakeMonitor.Regions.Application.Abstractions;
using EarthquakeMonitor.Regions.Domain;
using Microsoft.Extensions.Configuration;

namespace EarthquakeMonitor.Regions.Infrastructure;

public sealed class RegionCatalog(IConfiguration configuration) : IRegionCatalog
{
    private IReadOnlyList<RegionDefinition> Regions =>
        configuration.GetSection("Regions").Get<RegionSettings[]>()?.Select(ToDefinition).ToArray()
        ?? throw new InvalidOperationException("The Regions configuration section is missing or empty.");

    public RegionDefinition Get(RegionCode code) =>
        Regions.FirstOrDefault(region => region.Code == code)
        ?? throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown region.");

    public IReadOnlyList<RegionDefinition> GetAll() => Regions;

    public bool Contains(RegionCode code, decimal latitude, decimal longitude) =>
        code == RegionCode.Global || Get(code).Bounds!.Contains(latitude, longitude);

    private static RegionDefinition ToDefinition(RegionSettings settings)
    {
        if (!Enum.TryParse<RegionCode>(settings.Code, ignoreCase: true, out var code))
            throw new InvalidOperationException($"Unknown region code '{settings.Code}'.");

        var bounds = settings.Bounds is null ? null : new GeographicBounds(
            settings.Bounds.MinimumLatitude, settings.Bounds.MaximumLatitude,
            settings.Bounds.MinimumLongitude, settings.Bounds.MaximumLongitude);

        return new RegionDefinition(code, settings.Name, bounds);
    }

    private sealed class RegionSettings
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public BoundsSettings? Bounds { get; set; }
    }

    private sealed class BoundsSettings
    {
        public decimal MinimumLatitude { get; set; }
        public decimal MaximumLatitude { get; set; }
        public decimal MinimumLongitude { get; set; }
        public decimal MaximumLongitude { get; set; }
    }
}
