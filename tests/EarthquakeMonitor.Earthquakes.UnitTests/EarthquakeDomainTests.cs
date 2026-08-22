using EarthquakeMonitor.Earthquakes.Domain;
using Xunit;

namespace EarthquakeMonitor.Earthquakes.UnitTests;

public sealed class EarthquakeDomainTests
{
    [Fact]
    public void Create_rejects_coordinates_outside_valid_ranges()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Earthquake.Create("USGS", "id", DateTimeOffset.UtcNow, 91, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Earthquake.Create("USGS", "id", DateTimeOffset.UtcNow, 0, 181));
    }

    [Fact]
    public void Create_trims_and_keeps_source_identity()
    {
        var eventRecord = Earthquake.Create(" USGS ", " event-1 ", DateTimeOffset.UtcNow, 9, -84);

        Assert.Equal("USGS", eventRecord.Source);
        Assert.Equal("event-1", eventRecord.ExternalId);
    }
}
