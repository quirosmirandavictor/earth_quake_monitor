using EarthquakeMonitor.Earthquakes.Infrastructure.Sources.USGS;
using EarthquakeMonitor.Earthquakes.Infrastructure.Sources.USGS.Models;
using Xunit;

namespace EarthquakeMonitor.Earthquakes.UnitTests;

public sealed class UsgsEarthquakeMapperTests
{
    [Fact]
    public void Map_converts_usgs_coordinates_and_metadata()
    {
        var feature = new UsgsFeature
        {
            Id = "us-test-1",
            Properties = new UsgsProperties
            {
                Magnitude = 4.5m,
                MagnitudeType = "ml",
                Place = "10 km south of San Jose",
                TimeUnixMilliseconds = 1_700_000_000_000,
                UpdatedUnixMilliseconds = 1_700_000_001_000,
                Status = "reviewed",
                Tsunami = 0,
                Alert = "green",
                Significance = 120
            },
            Geometry = new UsgsGeometry
            {
                Type = "Point",
                Coordinates = [-84.08m, 9.93m, 12.4m]
            }
        };

        var result = new UsgsEarthquakeMapper().Map(feature);

        Assert.Equal("USGS", result.Source);
        Assert.Equal("us-test-1", result.ExternalId);
        Assert.Equal(9.93m, result.Latitude);
        Assert.Equal(-84.08m, result.Longitude);
        Assert.Equal(12.4m, result.DepthKm);
        Assert.Equal(4.5m, result.Magnitude);
        Assert.Equal("reviewed", result.ProviderStatus);
        Assert.Equal(0, result.Tsunami);
    }

    [Fact]
    public void Map_rejects_missing_identifier()
    {
        var feature = new UsgsFeature
        {
            Geometry = new UsgsGeometry { Type = "Point", Coordinates = [0m, 0m] }
        };

        Assert.Throws<InvalidDataException>(() => new UsgsEarthquakeMapper().Map(feature));
    }

    [Fact]
    public void Map_rejects_invalid_latitude()
    {
        var feature = new UsgsFeature
        {
            Id = "us-invalid",
            Properties = new UsgsProperties { TimeUnixMilliseconds = 1_700_000_000_000 },
            Geometry = new UsgsGeometry { Type = "Point", Coordinates = [0m, 91m] }
        };

        Assert.Throws<InvalidDataException>(() => new UsgsEarthquakeMapper().Map(feature));
    }
}
