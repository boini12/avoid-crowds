using AvoidCrowds.Api.SoccerCrowdCheck;
using NUnit.Framework;

namespace AvoidCrowds.Api.UnitTests.SoccerCrowdCheck;

[TestFixture]
public class BundesligaCitiesTests
{
    // Every club name OpenLigaDB reported across the bl1/bl2 seasons
    // 2024/25 - 2026/27, via getavailableteams. If OpenLigaDB renames a club
    // or a promoted club is missing from the curated list, that club's home
    // fixtures silently stop producing warnings - this pins the whole set so
    // the gap surfaces as a test failure instead.
    private static readonly string[] KnownHomeTeams =
    [
        "1. FC Heidenheim 1846", "1. FC Kaiserslautern", "1. FC Köln", "1. FC Magdeburg",
        "1. FC Nürnberg", "1. FC Union Berlin", "1. FSV Mainz 05", "Bayer 04 Leverkusen",
        "Borussia Dortmund", "Borussia Mönchengladbach", "DSC Arminia Bielefeld", "Dynamo Dresden",
        "Eintracht Braunschweig", "Eintracht Frankfurt", "Energie Cottbus", "FC Augsburg",
        "FC Bayern München", "FC Schalke 04", "FC St. Pauli", "Fortuna Düsseldorf",
        "Hamburger SV", "Hannover 96", "Hertha BSC", "Holstein Kiel",
        "Jahn Regensburg", "Karlsruher SC", "Preußen Münster", "RB Leipzig",
        "SC Freiburg", "SC Paderborn 07", "SSV Ulm 1846", "SV 07 Elversberg",
        "SV Darmstadt 98", "SV Werder Bremen", "SpVgg Greuther Fürth", "TSG Hoffenheim",
        "VfB Stuttgart", "VfL Bochum", "VfL Osnabrück", "VfL Wolfsburg",
    ];

    [TestCaseSource(nameof(KnownHomeTeams))]
    public void VenueCity_KnownHomeTeamWithoutLocation_ResolvesToACity(string homeTeam)
    {
        // Act
        var result = BundesligaCities.VenueCity(null, homeTeam);

        // Assert
        Assert.That(result, Is.Not.Null, $"'{homeTeam}' is not mapped to a home city.");
    }

    [Test]
    public void VenueCity_HomeTeamPlayingOutsideItsOwnNamesakeCity_ResolvesToTheVenueCity()
    {
        // Act
        var schalke = BundesligaCities.VenueCity(null, "FC Schalke 04");
        var hoffenheim = BundesligaCities.VenueCity(null, "TSG Hoffenheim");

        // Assert
        Assert.That(schalke!.Name, Is.EqualTo("Gelsenkirchen"));
        Assert.That(hoffenheim!.Name, Is.EqualTo("Sinsheim"));
    }

    [Test]
    public void VenueCity_UnknownHomeTeamWithoutLocation_ReturnsNull()
    {
        // Act
        var result = BundesligaCities.VenueCity(null, "FC Red Bull Salzburg");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void VenueCity_LocationCityTakesPrecedenceOverHomeTeam()
    {
        // Act: a home fixture moved to another Bundesliga city - the stated
        // location wins over the club's usual home city.
        var result = BundesligaCities.VenueCity("Köln", "Borussia Dortmund");

        // Assert
        Assert.That(result!.Name, Is.EqualTo("Köln"));
    }

    [Test]
    public void VenueCity_UnknownLocationCity_FallsBackToHomeTeam()
    {
        // Act
        var result = BundesligaCities.VenueCity("Salzburg", "Borussia Dortmund");

        // Assert
        Assert.That(result!.Name, Is.EqualTo("Dortmund"));
    }

    [Test]
    public void MatchesStop_FrankfurtMainStation_MatchesFrankfurtFixtureCity()
    {
        // Act
        var result = BundesligaCities.MatchesStop("Frankfurt (Main) Hbf", CityNamed("Frankfurt am Main"));

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesStop_FrankfurtOderStation_DoesNotMatchFrankfurtFixtureCity()
    {
        // Act: Frankfurt (Oder) is a distinct city with no Bundesliga 1/2 club - must
        // not be confused with Frankfurt am Main just because both station
        // names start with "Frankfurt".
        var result = BundesligaCities.MatchesStop("Frankfurt (Oder) Hbf", CityNamed("Frankfurt am Main"));

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesStop_StationInUnrelatedCity_ReturnsFalse()
    {
        // Act
        var result = BundesligaCities.MatchesStop("Hamburg Hbf", CityNamed("Berlin"));

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesStop_CaseInsensitive_StillMatches()
    {
        // Act
        var result = BundesligaCities.MatchesStop("berlin hbf", CityNamed("Berlin"));

        // Assert
        Assert.That(result, Is.True);
    }

    private static CityDefinition CityNamed(string name) => BundesligaCities.All.Single(city => city.Name == name);
}
