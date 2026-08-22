using EarthquakeMonitor.Regions.Domain;
using EarthquakeMonitor.Regions.Infrastructure;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EarthquakeMonitor.Earthquakes.UnitTests;

public sealed class RegionCatalogTests
{
    [Fact]
    public void Costa_Rica_contains_a_coordinate_inside_its_configured_bounds()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Regions:0:Code"] = "CostaRica",
            ["Regions:0:Name"] = "Costa Rica",
            ["Regions:0:Bounds:MinimumLatitude"] = "8",
            ["Regions:0:Bounds:MaximumLatitude"] = "11.3",
            ["Regions:0:Bounds:MinimumLongitude"] = "-86",
            ["Regions:0:Bounds:MaximumLongitude"] = "-82.5",
            ["Regions:1:Code"] = "Global",
            ["Regions:1:Name"] = "Global"
        }).Build();
        var catalog = new RegionCatalog(configuration);

        Assert.True(catalog.Contains(RegionCode.CostaRica, 9.93m, -84.08m));
        Assert.False(catalog.Contains(RegionCode.CostaRica, 40m, -84.08m));
    }
}
