using AvoidCrowds.Api.SoccerCrowdCheck;
using NUnit.Framework;

namespace AvoidCrowds.Api.UnitTests.SoccerCrowdCheck;

[TestFixture]
public class BundesligaCitiesTests
{
    [Test]
    public void MatchesStop_FrankfurtMainStation_MatchesFrankfurtFixtureCity()
    {
        // Act
        var result = BundesligaCities.MatchesStop("Frankfurt (Main) Hbf", "Frankfurt am Main");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesStop_FrankfurtOderStation_DoesNotMatchFrankfurtFixtureCity()
    {
        // Act: Frankfurt (Oder) is a distinct city with no Bundesliga 1/2 club - must
        // not be confused with Frankfurt am Main just because both station
        // names start with "Frankfurt".
        var result = BundesligaCities.MatchesStop("Frankfurt (Oder) Hbf", "Frankfurt am Main");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesStop_StationInUnrelatedCity_ReturnsFalse()
    {
        // Act
        var result = BundesligaCities.MatchesStop("Hamburg Hbf", "Berlin");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesStop_FixtureCityNotInCuratedList_ReturnsFalse()
    {
        // Act
        var result = BundesligaCities.MatchesStop("Salzburg Hbf", "Salzburg");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesStop_CaseInsensitive_StillMatches()
    {
        // Act
        var result = BundesligaCities.MatchesStop("berlin hbf", "BERLIN");

        // Assert
        Assert.That(result, Is.True);
    }
}
