using AvoidCrowds.Api.Journeys;
using AvoidCrowds.Api.SoccerCrowdCheck;
using NUnit.Framework;

namespace AvoidCrowds.Api.UnitTests.SoccerCrowdCheck;

[TestFixture]
public class CrowdMatcherTests
{
    private static readonly DateTimeOffset Kickoff = new(2026, 9, 20, 18, 30, 0, TimeSpan.FromHours(2));

    [Test]
    public void Match_StopInFixtureCityWithinWindow_ProducesWarning()
    {
        // Arrange
        var stop = new TripStop("Berlin Hbf", Kickoff.AddHours(1), null, "Europe/Berlin");
        var fixture = new Fixture("1. FC Union Berlin", "Hertha BSC", Kickoff, null);

        // Act
        var result = CrowdMatcher.Match([stop], [fixture]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].StopName, Is.EqualTo("Berlin Hbf"));
        Assert.That(result[0].HomeTeam, Is.EqualTo("1. FC Union Berlin"));
        Assert.That(result[0].AwayTeam, Is.EqualTo("Hertha BSC"));
        Assert.That(result[0].KickoffTime, Is.EqualTo(Kickoff));
        Assert.That(result[0].TimeZone, Is.EqualTo("Europe/Berlin"));
    }

    [Test]
    public void Match_StopExactlyThreeHoursBeforeKickoff_IsIncluded()
    {
        // Arrange
        var stop = new TripStop("Berlin Hbf", Kickoff.AddHours(-3), null, "Europe/Berlin");
        var fixture = new Fixture("1. FC Union Berlin", "Hertha BSC", Kickoff, null);

        // Act
        var result = CrowdMatcher.Match([stop], [fixture]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Match_StopExactlyThreeHoursAfterKickoff_IsIncluded()
    {
        // Arrange
        var stop = new TripStop("Berlin Hbf", Kickoff.AddHours(3), null, "Europe/Berlin");
        var fixture = new Fixture("1. FC Union Berlin", "Hertha BSC", Kickoff, null);

        // Act
        var result = CrowdMatcher.Match([stop], [fixture]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Match_StopJustOutsideThreeHourWindow_IsExcluded()
    {
        // Arrange
        var stop = new TripStop("Berlin Hbf", Kickoff.AddHours(3).AddMinutes(1), null, "Europe/Berlin");
        var fixture = new Fixture("1. FC Union Berlin", "Hertha BSC", Kickoff, null);

        // Act
        var result = CrowdMatcher.Match([stop], [fixture]);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Match_FixtureWithMissingLocation_StillMatchesViaHomeTeamCity()
    {
        // Arrange: OpenLigaDB returns a null location for every fixture, so the
        // home team has to carry the venue city on its own.
        var stop = new TripStop("Dortmund Hbf", Kickoff, null, "Europe/Berlin");
        var fixture = new Fixture("Borussia Dortmund", "SV Werder Bremen", Kickoff, null);

        // Act
        var result = CrowdMatcher.Match([stop], [fixture]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].StopName, Is.EqualTo("Dortmund Hbf"));
    }

    [Test]
    public void Match_FixtureWithUnknownHomeTeamAndNoLocation_IsExcluded()
    {
        // Arrange
        var stop = new TripStop("Berlin Hbf", Kickoff, null, "Europe/Berlin");
        var fixture = new Fixture("FC Somewhere Else", "Hertha BSC", Kickoff, null);

        // Act
        var result = CrowdMatcher.Match([stop], [fixture]);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Match_StopInDifferentCityFromFixture_IsExcluded()
    {
        // Arrange
        var stop = new TripStop("Hamburg Hbf", Kickoff, null, "Europe/Berlin");
        var fixture = new Fixture("1. FC Union Berlin", "Hertha BSC", Kickoff, null);

        // Act
        var result = CrowdMatcher.Match([stop], [fixture]);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Match_MultipleMatchingStopsAndFixtures_ProducesOneWarningPerMatch()
    {
        // Arrange
        var berlinStop = new TripStop("Berlin Hbf", Kickoff.AddMinutes(-30), Kickoff.AddMinutes(-25), "Europe/Berlin");
        var hamburgStop = new TripStop("Hamburg Hbf", Kickoff.AddHours(2), null, "Europe/Berlin");
        var berlinFixture = new Fixture("1. FC Union Berlin", "Hertha BSC", Kickoff, null);
        var hamburgFixture = new Fixture("Hamburger SV", "FC St. Pauli", Kickoff.AddHours(2), null);

        // Act
        var result = CrowdMatcher.Match([berlinStop, hamburgStop], [berlinFixture, hamburgFixture]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Select(warning => warning.StopName), Is.EquivalentTo(new[] { "Berlin Hbf", "Hamburg Hbf" }));
    }

    [Test]
    public void Match_NoStopsHaveMatchingFixture_ReturnsEmpty()
    {
        // Arrange
        var stop = new TripStop("Hannover Hbf", Kickoff, null, "Europe/Berlin");
        var fixture = new Fixture("1. FC Union Berlin", "Hertha BSC", Kickoff, null);

        // Act
        var result = CrowdMatcher.Match([stop], [fixture]);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Match_OriginAndDestinationStopsAreCheckedLikeAnyOtherStop()
    {
        // Arrange: origin has departure only, destination has arrival only - both must still be checked.
        var origin = new TripStop("Berlin Hbf", null, Kickoff, "Europe/Berlin");
        var destination = new TripStop("Hamburg Hbf", Kickoff.AddHours(2), null, "Europe/Berlin");
        var berlinFixture = new Fixture("1. FC Union Berlin", "Hertha BSC", Kickoff, null);
        var hamburgFixture = new Fixture("Hamburger SV", "FC St. Pauli", Kickoff.AddHours(2), null);

        // Act
        var result = CrowdMatcher.Match([origin, destination], [berlinFixture, hamburgFixture]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }
}
