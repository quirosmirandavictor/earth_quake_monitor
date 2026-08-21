using EarthquakeMonitor.Regions.Domain;

namespace EarthquakeMonitor.Regions.Application.Abstractions;

public interface IRegionCatalog
{
    RegionDefinition Get(RegionCode code);
    IReadOnlyList<RegionDefinition> GetAll();
    bool Contains(RegionCode code, decimal latitude, decimal longitude);
}
