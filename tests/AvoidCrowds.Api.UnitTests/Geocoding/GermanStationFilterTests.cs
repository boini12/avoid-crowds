using AvoidCrowds.Api.Geocoding;
using NUnit.Framework;

namespace AvoidCrowds.Api.UnitTests.Geocoding;

[TestFixture]
public class GermanStationFilterTests
{
    [Test]
    public void Filter_StopInsideGermanyBoundingBox_IsIncluded()
    {
        // Arrange
        var matches = new[]
        {
            new GeocodeMatch("STOP", "de:11000:900003200", "Berlin Hbf", 52.525, 13.369),
        };

        // Act
        var result = GermanStationFilter.Filter(matches);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Berlin Hbf"));
    }

    [Test]
    public void Filter_StopOutsideGermanyBoundingBox_IsExcluded()
    {
        // Arrange
        var matches = new[]
        {
            // "Berlin" ON, Kitchener, Canada
            new GeocodeMatch("STOP", "node/[260034778]", "Berlin", 43.451, -80.493),
        };

        // Act
        var result = GermanStationFilter.Filter(matches);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Filter_NonStopMatch_IsExcluded()
    {
        // Arrange
        var matches = new[]
        {
            new GeocodeMatch("PLACE", "node/[240109189]", "Berlin", 52.525, 13.369),
        };

        // Act
        var result = GermanStationFilter.Filter(matches);

        // Assert
        Assert.That(result, Is.Empty);
    }
}
