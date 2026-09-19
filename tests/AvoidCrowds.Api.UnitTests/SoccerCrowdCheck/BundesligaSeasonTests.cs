using AvoidCrowds.Api.SoccerCrowdCheck;
using NUnit.Framework;

namespace AvoidCrowds.Api.UnitTests.SoccerCrowdCheck;

[TestFixture]
public class BundesligaSeasonTests
{
    [TestCase(2025, 8, 15, 2025)]
    [TestCase(2025, 12, 1, 2025)]
    [TestCase(2026, 5, 1, 2025)]
    [TestCase(2026, 7, 1, 2026)]
    [TestCase(2026, 6, 30, 2025)]
    public void ForDate_ReturnsSeasonStartingYear(int year, int month, int day, int expectedSeason)
    {
        // Arrange
        var date = new DateTimeOffset(year, month, day, 12, 0, 0, TimeSpan.Zero);

        // Act
        var result = BundesligaSeason.ForDate(date);

        // Assert
        Assert.That(result, Is.EqualTo(expectedSeason));
    }
}
