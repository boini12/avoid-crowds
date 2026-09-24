using AvoidCrowds.Api.Journeys;

namespace AvoidCrowds.Api.SoccerCrowdCheck;

public static class CrowdMatcher
{
    private static readonly TimeSpan Window = TimeSpan.FromHours(3);

    public static IReadOnlyList<CrowdWarning> Match(IReadOnlyList<TripStop> stops, IEnumerable<Fixture> fixtures)
    {
        // Resolve each fixture's venue city once. A fixture whose home team is
        // in neither the curated club list nor a known location.city (a club
        // outside Bundesliga 1/2, or a name OpenLigaDB has since changed)
        // can't be tied to a venue city and is excluded up front.
        var locatedFixtures = fixtures
            .Select(fixture => (Fixture: fixture, City: BundesligaCities.GetVenueCity(fixture.LocationCity, fixture.HomeTeam)))
            .Where(located => located.City is not null)
            .ToList();

        var warnings = new List<CrowdWarning>();

        foreach (var stop in stops)
        {
            var stopTime = stop.StopTime;
            if (stopTime is null)
            {
                continue;
            }

            foreach (var (fixture, city) in locatedFixtures)
            {
                if (IsWithinWindow(stopTime.Value, fixture.KickoffTime)
                    && BundesligaCities.MatchesStop(stop.Name, city!))
                {
                    warnings.Add(new CrowdWarning(stop.Name, fixture.HomeTeam, fixture.AwayTeam, fixture.KickoffTime, stop.TimeZone));
                }
            }
        }

        return warnings;
    }

    private static bool IsWithinWindow(DateTimeOffset stopTime, DateTimeOffset kickoff) => (stopTime - kickoff).Duration() <= Window;
}
